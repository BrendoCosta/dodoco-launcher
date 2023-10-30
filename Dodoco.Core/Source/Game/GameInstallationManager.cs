namespace Dodoco.Core.Game;

using Dodoco.Core.Extension;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Util.Log;
using Dodoco.Core.Util.Hash;
using Dodoco.Core.Util.FileSystem;

using System.IO.Compression;
using System.Security.Cryptography;

public class GameInstallationManager: IGameInstallationManager {

    private IGameEx _Game;

    private GameInstallationManagerState _State;
    public GameInstallationManagerState State {
        get => this._State;
        protected set {
            Logger.GetInstance().Debug($"Updating {nameof(GameInstallationManagerState)} from {this._State.ToString()} to {value.ToString()}");
            this._State = value;
        }
    }

    /// <inheritdoc />
    public event EventHandler<GameInstallationManagerState> OnStateUpdate = delegate {};

    public GameInstallationManager(IGameEx game) => this._Game = game;

    /// <inheritdoc />
    public virtual async Task<long> GetGamePackageDownloadSize() {

        ResourceResponse latestResource = await this._Game.GetApiFactory().FetchLauncherResource();
        latestResource.EnsureSuccessStatusCode();
        return latestResource.data.game.latest.segments.Select(s => s.package_size).Sum();

    }

    /// <inheritdoc />
    public virtual async Task InstallGameAsync(ProgressReporter<ProgressReport>? progress, bool forceReinstall = false, CancellationToken token = default) {
        
        GameInstallationManagerState previousState = this.State;

        try {

            if (this._Game.CheckGameInstallation() && !forceReinstall)
                throw new GameException("The game is already installed in the selected installation directory");

            Logger.GetInstance().Log($"Installing the the game...");

            ResourceResponse latestResource = await this._Game.GetApiFactory().FetchLauncherResource();
            if (!latestResource.IsSuccessfull())
                throw new GameException($"Invalid resource");
            
            Logger.GetInstance().Log($"Downloading game's segments...");

            double totalBytesTransferred = 0.0D;

            for (int i = 0; i < latestResource.data.game.latest.segments.Count; i++) {

                ResourceSegment segment = latestResource.data.game.latest.segments[i];
                string segmentFileName = segment.path.Split("/").Last();

                if (!Directory.Exists(this._Game.Settings.InstallationDirectory))
                    Directory.CreateDirectory(this._Game.Settings.InstallationDirectory);

                if (File.Exists(Path.Join(this._Game.Settings.InstallationDirectory, segmentFileName))) {

                    this.State = GameInstallationManagerState.RECOVERING_DOWNLOADED_SEGMENTS;

                    ProgressReport report = new ProgressReport {
                        Done = i + 1,
                        Total = latestResource.data.game.latest.segments.Count,
                        Message = Path.Join(this._Game.Settings.InstallationDirectory, segmentFileName)
                    };

                    Logger.GetInstance().Log($"Checking the integrity of the game segment \"{segmentFileName}\"...");
                    string downloadedSegmentChecksum = new Hash(MD5.Create()).ComputeHash(Path.Join(this._Game.Settings.InstallationDirectory, segmentFileName));
                    
                    progress?.Report(report);

                    if (segment.md5.ToUpper() == downloadedSegmentChecksum.ToUpper()) {

                        Logger.GetInstance().Log($"The game segment \"{segmentFileName}\" is already downloaded and its MD5 checksum matchs the remote one, thus its download will be skipped");
                        totalBytesTransferred += (double) segment.package_size;
                        continue;

                    }

                }

                this.State = GameInstallationManagerState.DOWNLOADING_SEGMENTS;
                double doneFromLastReport = 0;

                ProgressReporter<ProgressReport> segmentProgress = new ProgressReporter<ProgressReport>();
                segmentProgress.ProgressChanged += (object? s, ProgressReport e) => {

                    totalBytesTransferred += e.Done - doneFromLastReport;
                    doneFromLastReport = e.Done;

                    ProgressReport generalProgress = new ProgressReport {
                        Done = totalBytesTransferred,
                        Total = latestResource.data.game.latest.package_size,
                        Rate = e.Rate,
                        Message = e.Message
                    };

                    if (e.Rate != null) {

                        generalProgress.EstimatedRemainingTime = TimeSpan.FromSeconds((double) (latestResource.data.game.latest.package_size - (double) totalBytesTransferred) / (double) e.Rate);

                    }

                    progress?.Report(generalProgress);

                };

                Logger.GetInstance().Log($"Downloading the game segment \"{segmentFileName}\" (segment no. {i+1} of {latestResource.data.game.latest.segments.Count})");
                
                Logger.GetInstance().Log($"Checking storage device available space...");
                long storageFreeBytes = FileSystem.GetAvailableStorageSpace(this._Game.Settings.InstallationDirectory); 
                if (segment.package_size > storageFreeBytes)
                    throw new GameException($"There is no enough storage space available to download the game's segment {i+1}. The download's process requires {DataUnitFormatter.Format(segment.package_size)} of storage space, but there is only {DataUnitFormatter.Format(storageFreeBytes)} available. Try freeing up storage space and restart the download; already downloaded files will not need to be redownloaded");

                await Client.GetInstance().DownloadFileAsync(new Uri(segment.path), Path.Join(this._Game.Settings.InstallationDirectory, segmentFileName), segmentProgress, token);

                string downloadedSegmentHash = new Hash(MD5.Create()).ComputeHash(Path.Join(this._Game.Settings.InstallationDirectory, segmentFileName));
                if (segment.md5.ToUpper() != downloadedSegmentHash.ToUpper()) {

                    throw new GameException($"Downloaded game segment \"{segmentFileName}\" MD5 checksum ({downloadedSegmentHash.ToUpper()}) doesn't match the expected remote checksum ({segment.md5.ToUpper()})");

                }

                Logger.GetInstance().Log($"Successfully downloaded the game segment \"{segmentFileName}\"");

            }

            Logger.GetInstance().Log($"Successfully downloaded all game's segments");
            
            this.State = GameInstallationManagerState.UNZIPPING_SEGMENTS;
            Logger.GetInstance().Log($"Unzipping downloaded game's segments...");

            // Sorts all game's segments and ensures the first one (...zip.001) is a zip archive

            latestResource.data.game.latest.segments.Sort((segmentA, segmentB) => segmentA.path.ToUpper().CompareTo(segmentB.path.ToUpper()));
            ResourceSegment firstSegment = latestResource.data.game.latest.segments.First();

            using (Stream fileStream = File.OpenRead(Path.Join(this._Game.Settings.InstallationDirectory, firstSegment.path.Split("/").Last()))) {

                const int ZIPFILE_MAGIC_NUMBER_BYTE_SIZE = 4;
                byte[] zipFileMagicNumber = new byte[ZIPFILE_MAGIC_NUMBER_BYTE_SIZE] { 0x50, 0x4B, 0x03, 0x04 };
                byte[] buffer = new byte[ZIPFILE_MAGIC_NUMBER_BYTE_SIZE] { 0x00, 0x00, 0x00, 0x00 };
                
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.ReadExactly(buffer, 0, ZIPFILE_MAGIC_NUMBER_BYTE_SIZE);
                
                if (!buffer.SequenceEqual(zipFileMagicNumber))
                    throw new GameException("The first game's segment is not a zip file");

            }

            /* Creates a single and memory-contiguous stream composed by the filestreams of
             * each segment file. This eliminates the need of joining all segments into
             * a single, big zip file before unzipping it, thereby saving disk space and
             * speeding up the whole process. */

            using (MultiStream.Lib.MultiStream segmentsMultiStream = new MultiStream.Lib.MultiStream(latestResource.data.game.latest.segments.Select(s => File.OpenRead(Path.Join(this._Game.Settings.InstallationDirectory, s.path.Split("/").Last()))))) {

                using (ZipArchive zipArchive = new ZipArchive(segmentsMultiStream, ZipArchiveMode.Read)) {

                    long storageFreeBytes = FileSystem.GetAvailableStorageSpace(this._Game.Settings.InstallationDirectory);
                    if (zipArchive.GetFullLength() > storageFreeBytes)
                        throw new GameException($"There is no enough storage space available to unzip the game's segments. This process requires {DataUnitFormatter.Format(zipArchive.GetFullLength())} of storage space, but there is only {DataUnitFormatter.Format(storageFreeBytes)} available. Try freeing up storage space and then try again");
                    
                    zipArchive.ExtractToDirectory(this._Game.Settings.InstallationDirectory, true, progress);

                }

            }

            Logger.GetInstance().Log($"Sucessfully unzipped downloaded game's segments");
            Logger.GetInstance().Log($"Successfully installed the game");

            this.State = previousState;
            return;

        } catch (CoreException e) {

            throw new GameException("Failed to install the game", e);

        } finally {

            this.State = previousState;

        }

    }

}