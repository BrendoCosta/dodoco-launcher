using Dodoco.Application;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Network.HTTP;
using Dodoco.Util.Log;
using Dodoco.Util.Hash;
using Dodoco.Util.Unit;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Game {

    public abstract class Game: IGame {

        public string InstallationDirectory { get; private set; }
        public GameServer GameServer { get; private set; }
        public GameState State { get; private set; }
        public Version Version { get; private set; }
        public Resource Resource { get; private set; }
        public event EventHandler<ApplicationProgressReport> OnCheckIntegrityProgress = delegate {};
        public event EventHandler<DownloadProgressReport> OnDownloadProgress = delegate {};

        public Game(Version version, GameServer server, Resource resource, string InstallationDirectory, GameState state) {

            this.InstallationDirectory = InstallationDirectory;
            this.State = state;
            this.Version = version;
            if (!resource.IsSuccessfull()) throw new GameException("Got an invalid resource");
            this.Resource = resource;

        }

        public virtual void SetInstallationDirectory(string directory) => this.InstallationDirectory = directory;
        public virtual void SetVersion(Version version) => this.Version = version;

        public virtual Version GetVersion() => this.Version;

        public virtual async Task Download(CancellationToken token = default) {

            if (this.State != GameState.WAITING_FOR_DOWNLOAD)
                throw new ForbiddenGameStateException(this.State);
            
            Logger.GetInstance().Log($"Starting game download...");
            
            ulong uncompressedGameSizeBytes = this.Resource.data.game.latest.size;
            ulong compressedGameSizeBytes = this.Resource.data.game.latest.package_size;
            int segmentCount = this.Resource.data.game.latest.segments.Count;
            ulong averageCompressedGameSegmentSizeBytes = compressedGameSizeBytes / Convert.ToUInt64(segmentCount);

            Logger.GetInstance().Log($"Uncompressed game size is {DataUnitFormatter.Format(uncompressedGameSizeBytes)}");
            Logger.GetInstance().Log($"Compressed game size is {DataUnitFormatter.Format(compressedGameSizeBytes)}");
            Logger.GetInstance().Log($"Average compressed game segment size is {DataUnitFormatter.Format(averageCompressedGameSegmentSizeBytes)}");

            Logger.GetInstance().Log($"Trying to find the given installation InstallationDirectory ({this.InstallationDirectory})");
            if (!Directory.Exists(this.InstallationDirectory)) throw new GameException("The given game installation InstallationDirectory doesn't exists");
            Logger.GetInstance().Log($"Successfully found installation InstallationDirectory");

            Logger.GetInstance().Log($"Checking storage device available space...");
            ulong storageFreeBytes = Convert.ToUInt64(new DriveInfo(this.InstallationDirectory).AvailableFreeSpace); 
            Logger.GetInstance().Log($"There is {DataUnitFormatter.Format(storageFreeBytes)} of space available in the storage device");
            
            if (storageFreeBytes <= uncompressedGameSizeBytes) throw new GameException("There is not enough space available in the storage device for game installation");
            Logger.GetInstance().Log($"There is enough space available in storage device for game installation");

            Logger.GetInstance().Log($"Downloading game segments...");
            
            for (int i = 0; i < segmentCount; i++) {

                Resource.Segment segment = this.Resource.data.game.latest.segments[i];
                string fileName = segment.path.Split("/").Last();

                Logger.GetInstance().Log($"Downloading game segments ({i+1} of {segmentCount})...");
                Logger.GetInstance().Log($"{segment.path.Split("/").Last()}");

            }

            Logger.GetInstance().Log($"Successfully downloaded game segments");

        }

        public virtual async Task<List<GameIntegrityReport>> CheckIntegrity(CancellationToken token = default) {

            if ((this.State != GameState.READY) && (this.State != GameState.WAITING_FOR_UPDATE))
                throw new ForbiddenGameStateException(this.State);

            GameState savedState = this.State;
            this.UpdateState(GameState.CHECKING_INTEGRITY);
            
            Logger.GetInstance().Log($"Starting game integrity check...");
            
            Uri decompressedPathUrl =
                this.Resource.data.game.latest.decompressed_path.ToString().EndsWith("/")
                ? this.Resource.data.game.latest.decompressed_path
                : new Uri(this.Resource.data.game.latest.decompressed_path.ToString() + "/");

            Uri pkgVersionRemoteUrl = new Uri(decompressedPathUrl, "pkg_version");
            HttpResponseMessage response = await Application.Application.GetInstance().client.FetchAsync(pkgVersionRemoteUrl);
            List<GamePkgVersionEntry> entries = new List<GamePkgVersionEntry>();
            List<GameIntegrityReport> mismatches = new List<GameIntegrityReport>();
            double pkgVersionTotalPackageSize = 0;
            double totalBytesRead = 0;
            List<double> estimatedRemainingTime = new List<double>();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            if (response.IsSuccessStatusCode) {

                string pkgVersionTextContent = await response.Content.ReadAsStringAsync();
                string? line;

                using (StringReader reader = new StringReader(await response.Content.ReadAsStringAsync())) {

                    while ((line = reader.ReadLine()) != null) {

                        GamePkgVersionEntry entry = JsonSerializer.Deserialize<GamePkgVersionEntry>(line);
                        entries.Add(entry);
                        
                        pkgVersionTotalPackageSize += (double) entry.fileSize;

                    }

                }

                Logger.GetInstance().Log($"pkg_version total size is {DataUnitFormatter.Format(pkgVersionTotalPackageSize)}");
                
                for (int i = 0; i < entries.Count; i++) {

                    GamePkgVersionEntry currentEntry = entries[i];
                    FileInfo file = new FileInfo(Path.Join(this.InstallationDirectory, currentEntry.remoteName));
                    Logger.GetInstance().Log($"Checking the file \"{file.FullName}\"...");
                    
                    if (file.Exists) {

                        string localHash = MD5.ComputeHash(file).ToUpper();

                        if (localHash != currentEntry.md5.ToUpper()) {

                            mismatches.Add(new GameIntegrityReport {

                                localFileIntegrityState = GameFileIntegrityState.CORRUPTED,
                                localFilePath = file.FullName,
                                localFileHash = localHash,
                                localFileSize = (ulong) file.Length,
                                remoteFilePath = currentEntry.remoteName,
                                remoteFileHash = currentEntry.md5.ToUpper(),
                                remoteFileSize = currentEntry.fileSize

                            });

                        }

                        totalBytesRead += file.Length;
                        estimatedRemainingTime.Add(((pkgVersionTotalPackageSize - totalBytesRead) * watch.Elapsed.TotalSeconds) / totalBytesRead);

                    } else {

                        mismatches.Add(new GameIntegrityReport {

                            localFileIntegrityState = GameFileIntegrityState.MISSING,
                            localFilePath = file.FullName,
                            localFileHash = string.Empty,
                            localFileSize = 0,
                            remoteFilePath = currentEntry.remoteName,
                            remoteFileHash = currentEntry.md5.ToUpper(),
                            remoteFileSize = currentEntry.fileSize

                        });

                    }

                    ApplicationProgressReport report = new ApplicationProgressReport {

                        completionPercentage = ((double) i / (double) entries.Count) * 100.0D,
                        estimatedRemainingTime = TimeSpan.FromSeconds(estimatedRemainingTime.Count >= 2 ? estimatedRemainingTime.Average() : 1)

                    };

                    this.OnCheckIntegrityProgress.Invoke(this, report);

                    if (estimatedRemainingTime.Count > 9) estimatedRemainingTime.Clear();

                }

            } else {

                throw new GameException($"Failed to fetch the pkg_version file from remote servers (received HTTP status code {response.StatusCode})");

            }

            watch.Stop();

            Logger.GetInstance().Log($"Successfully finished game integrity check ({DataUnitFormatter.Format(totalBytesRead)} read with {mismatches.Count} mismatches found)");

            foreach (var report in mismatches) {

                switch (report.localFileIntegrityState) {

                    case GameFileIntegrityState.CORRUPTED:
                        Logger.GetInstance().Log($"Mismatch: the local file \"{report.localFilePath}\" MD5 hash ({report.localFileHash}) doesn't match the expected remote hash ({report.remoteFileHash})");
                        break;
                    
                    case GameFileIntegrityState.MISSING:
                        Logger.GetInstance().Log($"Mismatch: the local file \"{report.localFilePath}\" is missing");
                        break;

                }

            }

            this.UpdateState(savedState);

            return mismatches;

        }

        private void UpdateState(GameState newState) {

            Logger.GetInstance().Debug($"Updating game state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;

        }

    }

}