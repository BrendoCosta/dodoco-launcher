using Dodoco.Core.Network;
using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Extension;
using Dodoco.Core.Util.Log;
using Dodoco.Core.Util.Hash;
using Dodoco.Core.Util.FileSystem;
using Dodoco.Core.Wine;

using UrlCombineLib;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Hi3Helper.SharpHDiffPatch;

namespace Dodoco.Core.Game {

    public class Game: IGame {

        public GameSettings Settings { get; set; }
        private GameDownloadState _DownloadState { get; set; } = GameDownloadState.DOWNLOADED;
        public GameDownloadState DownloadState {
            get => this._DownloadState;
            protected set {
                Logger.GetInstance().Debug($"Updating game download state from {this._DownloadState.ToString()} to {value.ToString()}");
                this._DownloadState = value;
            }
        }
        private GameUpdateState _UpdateState { get; set; } = GameUpdateState.UPDATED;
        public GameUpdateState UpdateState {
            get => this._UpdateState;
            protected set {
                Logger.GetInstance().Debug($"Updating game update state from {this._UpdateState.ToString()} to {value.ToString()}");
                this._UpdateState = value;
            }
        }
        private GameIntegrityCheckState _IntegrityCheckState { get; set; } = GameIntegrityCheckState.IDLE;
        public GameIntegrityCheckState IntegrityCheckState {
            get => this._IntegrityCheckState;
            protected set {
                Logger.GetInstance().Debug($"Updating game integrity check state from {this._IntegrityCheckState.ToString()} to {value.ToString()}");
                this._IntegrityCheckState = value;
            }
        }
        public GameServer GameServer { get; protected set; }
        public bool IsInstalled { get => GameInstallationManager_Old.CheckGameInstallation(this.Settings.InstallationDirectory, this.Settings.Server); }
        public GameState State { get; private set; } = GameState.READY;
        public IWine? Wine { get; set; }
        public ResourceResponse Resource { get; private set; }
        public Version Version { get => Version.Parse(this.Resource.data.game.latest.version); }

        public event EventHandler<GameState> OnStateUpdate = delegate {};
        public event EventHandler AfterGameDownload = delegate {};
        public event EventHandler AfterGameUpdate = delegate {};

        private CompanyApiFactory apiFactory {
        
            get => new CompanyApiFactory(
                this.Settings.Api[this.Settings.Server].Url,
                this.Settings.Api[this.Settings.Server].Key,
                this.Settings.Api[this.Settings.Server].LauncherId,
                this.Settings.Language
            );

        }

        public Game(GameSettings settings, ResourceResponse resource) {

            this.Settings = settings;
            this.Resource = resource;

        }

        public virtual async Task<ResourceGame?> GetUpdateAsync() {

            ResourceResponse latestResource = await this.apiFactory.FetchLauncherResource();

            if (!latestResource.IsSuccessfull())
                throw new GameException("Invalid resource");

            if (Version.Parse(latestResource.data.game.latest.version) > GameInstallationManager_Old.SearchForGameVersion(this.Settings.InstallationDirectory, this.Settings.Server))
                return latestResource.data.game;

            return null;

        }

        public virtual async Task<ResourceGame?> GetPreUpdateAsync() {

            ResourceResponse latestResource = await this.apiFactory.FetchLauncherResource();
            
            if (!latestResource.IsSuccessfull())
                throw new GameException("Invalid resource");

            return latestResource.data.pre_download_game;

        }

        public virtual async Task<bool> IsPreUpdateDownloadedAsync() {

            ResourceGame? gameResource = await this.GetPreUpdateAsync();

            if (gameResource != null) {

                Version remoteVersion = Version.Parse(((ResourceGame) gameResource).latest.version);
                string packageFilenamePattern = @$"(game_{this.Version.ToString().Replace(".", @"\.")}_{remoteVersion.ToString().Replace(".", @"\.")}_hdiff_(\w*)\.zip)";

                if (Directory.EnumerateFiles(this.Settings.InstallationDirectory).ToList().Exists(someFile => Regex.IsMatch(Path.GetFileName(someFile), packageFilenamePattern)))
                    return true;

            }

            return false;

        }

        public virtual async Task Download(ProgressReporter<ProgressReport>? progress, CancellationToken token = default) {

            GameDownloadState previousState = this.DownloadState;

            try {

                if (this.IsInstalled)
                    throw new GameException("Game is already installed");
            
                Logger.GetInstance().Log($"Starting game download...");

                ResourceResponse latestResource = await this.apiFactory.FetchLauncherResource();
                if (!latestResource.IsSuccessfull())
                    throw new GameException($"Invalid resource");
                
                Logger.GetInstance().Log($"Downloading game's segments...");

                double totalBytesTransferred = 0.0D;

                for (int i = 0; i < latestResource.data.game.latest.segments.Count; i++) {

                    ResourceSegment segment = latestResource.data.game.latest.segments[i];
                    string segmentFileName = segment.path.Split("/").Last();

                    if (!Directory.Exists(this.Settings.InstallationDirectory))
                        Directory.CreateDirectory(this.Settings.InstallationDirectory);

                    if (File.Exists(Path.Join(this.Settings.InstallationDirectory, segmentFileName))) {

                        this.DownloadState = GameDownloadState.RECOVERING_DOWNLOADED_SEGMENTS;

                        ProgressReport report = new ProgressReport {
                            Done = i + 1,
                            Total = latestResource.data.game.latest.segments.Count,
                            Message = Path.Join(this.Settings.InstallationDirectory, segmentFileName)
                        };

                        Logger.GetInstance().Log($"Checking the integrity of the game segment \"{segmentFileName}\"...");
                        string downloadedSegmentChecksum = new Hash(MD5.Create()).ComputeHash(Path.Join(this.Settings.InstallationDirectory, segmentFileName));
                        
                        progress?.Report(report);

                        if (segment.md5.ToUpper() == downloadedSegmentChecksum.ToUpper()) {

                            Logger.GetInstance().Log($"The game segment \"{segmentFileName}\" is already downloaded and its MD5 checksum matchs the remote one, thus its download will be skipped");
                            totalBytesTransferred += (double) segment.package_size;
                            continue;

                        }

                    }

                    this.DownloadState = GameDownloadState.DOWNLOADING_SEGMENTS;
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
                    long storageFreeBytes = FileSystem.GetAvailableStorageSpace(this.Settings.InstallationDirectory); 
                    if (segment.package_size > storageFreeBytes)
                        throw new GameException($"There is no enough storage space available to download the game's segment {i+1}. The download's process requires {DataUnitFormatter.Format(segment.package_size)} of storage space, but there is only {DataUnitFormatter.Format(storageFreeBytes)} available. Try freeing up storage space and restart the download; already downloaded files will not need to be redownloaded");

                    await Client.GetInstance().DownloadFileAsync(new Uri(segment.path), Path.Join(this.Settings.InstallationDirectory, segmentFileName), segmentProgress, token);

                    string downloadedSegmentHash = new Hash(MD5.Create()).ComputeHash(Path.Join(this.Settings.InstallationDirectory, segmentFileName));
                    if (segment.md5.ToUpper() != downloadedSegmentHash.ToUpper()) {

                        throw new GameException($"Downloaded game segment \"{segmentFileName}\" MD5 checksum ({downloadedSegmentHash.ToUpper()}) doesn't match the expected remote checksum ({segment.md5.ToUpper()})");

                    }

                    Logger.GetInstance().Log($"Successfully downloaded the game segment \"{segmentFileName}\"");

                }

                Logger.GetInstance().Log($"Successfully downloaded all game's segments");

                // Sorts all game's segments and ensures the first one (...zip.001) is a zip archive

                latestResource.data.game.latest.segments.Sort((segmentA, segmentB) => segmentA.path.ToUpper().CompareTo(segmentB.path.ToUpper()));
                ResourceSegment firstSegment = latestResource.data.game.latest.segments.First();

                using (Stream fileStream = File.OpenRead(Path.Join(this.Settings.InstallationDirectory, firstSegment.path.Split("/").Last()))) {

                    const int ZIPFILE_MAGIC_NUMBER_BYTE_SIZE = 4;
                    byte[] zipFileMagicNumber = new byte[ZIPFILE_MAGIC_NUMBER_BYTE_SIZE] { 0x50, 0x4B, 0x03, 0x04 };
                    byte[] buffer = new byte[ZIPFILE_MAGIC_NUMBER_BYTE_SIZE] { 0x00, 0x00, 0x00, 0x00 };
                    
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.ReadExactly(buffer, 0, ZIPFILE_MAGIC_NUMBER_BYTE_SIZE);
                    
                    if (!buffer.SequenceEqual(zipFileMagicNumber))
                        throw new GameException("The first game's segment is not a zip file");

                }

                this.DownloadState = GameDownloadState.UNZIPPING_SEGMENTS;
                Logger.GetInstance().Log($"Unzipping downloaded game's segments...");

                /* Creates a single and memory-contiguous stream composed by the filestreams of
                 * each segment file. This eliminates the need of joining all segments into
                 * a single, big zip file before unzipping it, thereby saving disk space and
                 * speeding up the whole process. */

                using (MultiStream.Lib.MultiStream segmentsMultiStream = new MultiStream.Lib.MultiStream(latestResource.data.game.latest.segments.Select(s => File.OpenRead(Path.Join(this.Settings.InstallationDirectory, s.path.Split("/").Last()))))) {

                    using (ZipArchive zipArchive = new ZipArchive(segmentsMultiStream, ZipArchiveMode.Read)) {

                        long storageFreeBytes = FileSystem.GetAvailableStorageSpace(this.Settings.InstallationDirectory);
                        if (zipArchive.GetFullLength() > storageFreeBytes)
                            throw new GameException($"There is no enough storage space available to unzip the game's segments. This process requires {DataUnitFormatter.Format(zipArchive.GetFullLength())} of storage space, but there is only {DataUnitFormatter.Format(storageFreeBytes)} available. Try freeing up storage space and restart the download; already downloaded files will not need to be redownloaded");
                        
                        zipArchive.ExtractToDirectory(this.Settings.InstallationDirectory, true, progress);

                    }

                }

                Logger.GetInstance().Log($"Sucessfully unzipped downloaded game's segments...");

                Logger.GetInstance().Log($"Successfully downloaded the game");
                this.DownloadState = previousState;
                this.AfterGameDownload.Invoke(this, EventArgs.Empty);
                return;

            } catch (Exception) {

                this.DownloadState = previousState;
                throw;

            }

        }

        public virtual async Task UpdateAsync(bool isPreUpdate, ProgressReporter<ProgressReport>? reporter = null, CancellationToken token = default) {

            if (!this.IsInstalled)
                throw new GameException("Game is not installed");

            GameUpdateState previousState = this.UpdateState;

            try {

                ResourceGame gameResource = isPreUpdate ? await this.GetPreUpdateAsync() ?? throw new GameException("Game pre-update is not available") : await this.GetUpdateAsync() ?? throw new GameException("Game update is not available");

                Version remoteVersion = Version.Parse(gameResource.latest.version);
                string packageFilenamePattern = @$"(game_{this.Version.ToString().Replace(".", @"\.")}_{remoteVersion.ToString().Replace(".", @"\.")}_hdiff_(\w*)\.zip)";

                Logger.GetInstance().Log($"{(isPreUpdate ? "Pre-updating" : "Updating")} the game to version {remoteVersion.ToString()}...");

                if (gameResource.diffs.Exists(d => Regex.IsMatch(d.name, packageFilenamePattern))) {

                    Task repairTask = this.RepairGameFiles(reporter, token);

                    if (!isPreUpdate)
                        repairTask.Start();

                    ResourceDiff diff = gameResource.diffs.Find(d => Regex.IsMatch(d.name, packageFilenamePattern));
                    string packageFileFullPath = Path.Join(this.Settings.InstallationDirectory, diff.name);
                    await this.DownloadUpdatePackageAsync(diff, reporter, token);
                    
                    if (!isPreUpdate) {

                        await repairTask;

                        this.UnzipUpdatePackage(packageFileFullPath, reporter, token);
                        this.ApplyUpdatePackagePatches(reporter, token);
                        this.RemoveDeprecatedFiles(reporter, token);

                    }

                } else {

                    throw new GameException($"Can't find a diff object whose name matchs the string pattern \"{packageFilenamePattern}\"");

                }
                
                Logger.GetInstance().Log($"Sucessfully {(isPreUpdate ? "pre-updated" : "updated")} the game to version {remoteVersion.ToString()}");
                this.UpdateState = previousState;
                this.AfterGameUpdate.Invoke(this, EventArgs.Empty);
                return;

            } catch (Exception) {

                this.UpdateState = previousState;
                throw;

            }

        }
        
        protected virtual async Task DownloadUpdatePackageAsync(ResourceDiff diff, ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

            GameUpdateState previousState = this.UpdateState;

            try {

                string packageFileFullPath = Path.Join(this.Settings.InstallationDirectory, diff.name);

                /*
                    * Downloads the update's package or skips
                    * its download if already in game's directory
                */

                this.UpdateState = GameUpdateState.DOWNLOADING_UPDATE_PACKAGE;

                if (File.Exists(packageFileFullPath) && new Hash(MD5.Create()).ComputeHash(packageFileFullPath).ToUpper() == diff.md5.ToUpper()) {

                    Logger.GetInstance().Log($"Found the game's update's package already inside the game's installation directory, skipping the download");

                } else {

                    try {

                        long availableStorageSpace = FileSystem.GetAvailableStorageSpace(this.Settings.InstallationDirectory);
                        if (diff.size >= availableStorageSpace) {

                            throw new GameException($"There is no enough storage space available to download the game update. This update requires {DataUnitFormatter.Format(diff.size)} of storage space, but there is only {DataUnitFormatter.Format(availableStorageSpace)} available");

                        }

                        await Client.GetInstance().DownloadFileAsync(new Uri(diff.path), packageFileFullPath, reporter, CancellationToken.None);
                        
                        Logger.GetInstance().Log($"Finished downloading the game's update package");
                        Logger.GetInstance().Log($"Checking game's update package's integrity...");

                        string downloadedPackageFileChecksum = new Hash(MD5.Create()).ComputeHash(packageFileFullPath);

                        if (downloadedPackageFileChecksum.ToUpper() == diff.md5.ToUpper()) {

                            Logger.GetInstance().Log($"The downloaded game's update package's checksum match the expected remote checksum");

                        } else {

                            throw new GameException($"The downloaded file's checksum ({downloadedPackageFileChecksum}) doesn't match the expected remote checksum ({diff.md5.ToUpper()})");

                        }

                    } catch (NetworkException e) {

                        throw new GameException($"Failed to download game's update package due to a network error", e);

                    }

                }

                Logger.GetInstance().Log($"Sucessfully downloaded game's update package");
                this.UpdateState = previousState;
                return;

            } catch (Exception) {

                this.UpdateState = previousState;
                throw;

            }

        }

        protected virtual void UnzipUpdatePackage(string packagePath, ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

            if (string.IsNullOrWhiteSpace(packagePath))
                throw new GameException("Empty package's path");

            if (!File.Exists(packagePath))
                throw new GameException("The update package is missing");

            GameUpdateState previousState = this.UpdateState;

            try {

                /*
                 * Removes old update package's files
                */

                GameHdiffFiles hdiffFilesHandler = new GameHdiffFiles(this.Settings.InstallationDirectory);
                GameDeleteFiles deleteFilesHandler = new GameDeleteFiles(this.Settings.InstallationDirectory);

                if (hdiffFilesHandler.Exist())
                    hdiffFilesHandler.Delete();
                
                if (deleteFilesHandler.Exist())
                    deleteFilesHandler.Delete();

                using (FileStream zipFileStream = File.OpenRead(packagePath)) {

                    /*
                     * Unzips the update package
                    */

                    this.UpdateState = GameUpdateState.UNZIPPING_UPDATE_PACKAGE;
                    Logger.GetInstance().Log($"Unzipping the game's update package...");
                    
                    try {

                        using(ZipArchive zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read)) {

                            // overwrite files = true
                            zipArchive.ExtractToDirectory(this.Settings.InstallationDirectory, true, reporter);

                        }
                        

                    } catch (Exception e) {

                        throw new GameException($"Failed to unzip the game's update package", e);

                    }
                    
                    Logger.GetInstance().Log($"Successfully finished unzipping the game's update package");
                    this.UpdateState = previousState;
                    return;

                }

            } catch (Exception) {

                this.UpdateState = previousState;
                throw;

            }

        }

        protected virtual void ApplyUpdatePackagePatches(ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

            GameHdiffFiles hdiffFilesHandler = new GameHdiffFiles(this.Settings.InstallationDirectory);

            if (!hdiffFilesHandler.Exist())
                throw new GameException($"Missing \"{hdiffFilesHandler.FileName}\" file");

            GameUpdateState previousState = this.UpdateState;

            try {

                /*
                 * Reads the "hdifffiles.txt" file and apply the patches for every referenced file
                */

                this.UpdateState = GameUpdateState.APPLYING_UPDATE_PACKAGE;
                Logger.GetInstance().Log($"Applying game's update package patches...");

                HDiffPatch patcher = new HDiffPatch();
                int appliedPatchesCount = 0;
                List<GameHdiffFilesEntry> patchesList = hdiffFilesHandler.Read();

                foreach (GameHdiffFilesEntry entry in patchesList) {

                    string oldFilePath = Path.Join(this.Settings.InstallationDirectory, entry.remoteName);
                    string oldFileBackupPath = Path.Join(this.Settings.InstallationDirectory, entry.remoteName + ".bak");
                    string patchFilePath = Path.Join(this.Settings.InstallationDirectory, entry.remoteName + ".hdiff");
                    
                    try {

                        Logger.GetInstance().Log($"Patching the file \"{oldFilePath}\"...");

                        // Creates a backup of the file from current game's version
                        File.Copy(oldFilePath, oldFileBackupPath);

                        // Loads its patch file into the patcher
                        patcher.Initialize(patchFilePath);

                        // Patches the old file (it becomes the newer/updated file)
                        patcher.Patch(oldFileBackupPath, oldFilePath, true);

                        // Removes the backup file and the patch file
                        File.Delete(oldFileBackupPath);
                        File.Delete(patchFilePath);

                        Logger.GetInstance().Log($"Successfully patched the file \"{oldFilePath}\"");

                        appliedPatchesCount++;
                        reporter?.Report(new ProgressReport {
                            Done = appliedPatchesCount,
                            Total = patchesList.Count,
                            Message = oldFilePath
                        });

                    } catch (Exception e) {

                        Logger.GetInstance().Error($"Failed to patch the file \"{oldFilePath}\"", e);

                    }

                }

                Logger.GetInstance().Log($"Sucessfully applied game's update package patches");
                this.UpdateState = previousState;
                return;

            } catch (Exception) {

                this.UpdateState = previousState;
                throw;

            }

        }

        protected virtual void RemoveDeprecatedFiles(ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

            GameDeleteFiles deleteFilesHandler = new GameDeleteFiles(this.Settings.InstallationDirectory);

            if (!deleteFilesHandler.Exist())
                throw new GameException($"Missing \"{deleteFilesHandler.FileName}\" file");

            GameUpdateState previousState = this.UpdateState;

            try {

                /*
                 * Reads the "deletefiles.txt" file and deletes every listed file
                */

                this.UpdateState = GameUpdateState.REMOVING_DEPRECATED_FILES;
                Logger.GetInstance().Log($"Removing game's deprecated files...");

                int removedFilesCount = 0;
                List<string> filesToRemove = deleteFilesHandler.Read();

                foreach (string filePath in filesToRemove) {

                    string fullPath = Path.Join(this.Settings.InstallationDirectory, filePath);

                    try {

                        Logger.GetInstance().Log($"Removing the file \"{fullPath}\"...");

                        File.Delete(fullPath);
                        
                        Logger.GetInstance().Log($"Successfully removed the file \"{fullPath}\"");

                        removedFilesCount++;
                        reporter?.Report(new ProgressReport {
                            Done = removedFilesCount,
                            Total = filesToRemove.Count,
                            Message = fullPath
                        });

                    } catch (Exception e) {

                        Logger.GetInstance().Error($"Failed to remove the file \"{fullPath}\"", e);

                    }

                }

                Logger.GetInstance().Log($"Sucessfully removed game's deprecated files");
                this.UpdateState = previousState;
                return;

            } catch (Exception) {

                this.UpdateState = previousState;
                throw;

            }

        }

        public virtual async Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(CancellationToken token = default) => await this.CheckFilesIntegrity(null, token);
        public virtual async Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(ProgressReporter<ProgressReport>? progress, CancellationToken token = default) {

            GameIntegrityCheckState previousState = this.IntegrityCheckState;

            try {

                this.IntegrityCheckState = GameIntegrityCheckState.CHECKING_INTEGRITY;

                Logger.GetInstance().Log($"Starting game integrity check...");
                
                Uri pkgVersionRemoteUrl = new Uri(UrlCombine.Combine(this.Resource.data.game.latest.decompressed_path.ToString(), "pkg_version"));
                HttpResponseMessage response = await Client.GetInstance().FetchAsync(pkgVersionRemoteUrl);
                List<GamePkgVersionEntry> entries = new List<GamePkgVersionEntry>();
                List<GameFileIntegrityReport> mismatches = new List<GameFileIntegrityReport>();
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
                        FileInfo file = new FileInfo(Path.Join(this.Settings.InstallationDirectory, currentEntry.remoteName));
                        
                        if (file.Exists) {

                            string localHash = new Hash(MD5.Create()).ComputeHash(file.FullName);

                            if (localHash.ToUpper() != currentEntry.md5.ToUpper()) {

                                mismatches.Add(new GameFileIntegrityReport {

                                    localFileIntegrityState = GameFileIntegrityState.CORRUPTED,
                                    localFilePath = file.FullName,
                                    localFileHash = localHash,
                                    localFileSize = (ulong) file.Length,
                                    remoteFilePath = currentEntry.remoteName,
                                    remoteFileHash = currentEntry.md5.ToUpper(),
                                    remoteFileSize = (ulong) currentEntry.fileSize

                                });

                            }

                            totalBytesRead += file.Length;
                            estimatedRemainingTime.Add(((pkgVersionTotalPackageSize - totalBytesRead) * watch.Elapsed.TotalSeconds) / totalBytesRead);

                        } else {

                            mismatches.Add(new GameFileIntegrityReport {

                                localFileIntegrityState = GameFileIntegrityState.MISSING,
                                localFilePath = file.FullName,
                                localFileHash = string.Empty,
                                localFileSize = 0,
                                remoteFilePath = currentEntry.remoteName,
                                remoteFileHash = currentEntry.md5.ToUpper(),
                                remoteFileSize = (ulong) currentEntry.fileSize

                            });

                        }

                        ProgressReport report = new ProgressReport {
                            Done = i + 1,
                            Total = entries.Count,
                            EstimatedRemainingTime = TimeSpan.FromSeconds(estimatedRemainingTime.Count >= 2 ? estimatedRemainingTime.Average() : 1),
                            Message = file.FullName
                        };

                        progress?.Report(report);

                        if (estimatedRemainingTime.Count > 9) estimatedRemainingTime.Clear();

                    }

                } else {

                    throw new GameException($"Failed to fetch the pkg_version file from remote servers (received HTTP status code {response.StatusCode})");

                }

                watch.Stop();

                Logger.GetInstance().Log($"Successfully finished game integrity check ({DataUnitFormatter.Format(totalBytesRead)} read with {mismatches.Count} mismatches found)");

                this.IntegrityCheckState = previousState;
                return mismatches;

            } catch (Exception) {

                this.IntegrityCheckState = previousState;
                throw;

            }

        }

        public async Task RepairFile(GameFileIntegrityReport report, CancellationToken token = default) => await this.RepairFile(report, null, token);
        public async Task RepairFile(GameFileIntegrityReport report, ProgressReporter<ProgressReport>? progress, CancellationToken token = default) {

            GameIntegrityCheckState previousState = this.IntegrityCheckState;

            try {

                this.IntegrityCheckState = GameIntegrityCheckState.DOWNLOADING_FILE;

                Logger.GetInstance().Log($"Trying to repair the game file \"{report.localFilePath}\"...");

                FileInfo localFile = new FileInfo(Path.Join(this.Settings.InstallationDirectory, report.remoteFilePath));
                FileInfo tempFile = new FileInfo(Path.Join(this.Settings.InstallationDirectory, Path.GetDirectoryName(report.remoteFilePath), Path.GetFileName(report.localFilePath) + ".temp"));
                Uri fileRemoteUrl = new Uri(UrlCombine.Combine(this.Resource.data.game.latest.decompressed_path.ToString(), report.remoteFilePath));

                Logger.GetInstance().Log($"Downloading a copy of the remote file to \"{tempFile.FullName}\"");
                
                await Client.GetInstance().DownloadFileAsync(fileRemoteUrl, tempFile.FullName, progress, token);
                
                this.IntegrityCheckState = GameIntegrityCheckState.REPAIRING_FILE;

                Logger.GetInstance().Log($"Computing the hash of the downloaded file...");

                string tempFileHash = new Hash(MD5.Create()).ComputeHash(tempFile.FullName);
                string remoteFileHash = report.remoteFileHash;
                
                if (tempFileHash.ToUpper() == remoteFileHash.ToUpper()) {

                    Logger.GetInstance().Log($"The downloaded file's hash match the expected remote hash");

                    localFile.Delete();
                    tempFile.MoveTo(localFile.FullName);

                    Logger.GetInstance().Log($"Successfully repaired the game file \"{report.localFilePath}\"");

                } else {

                    Logger.GetInstance().Error($"The downloaded file's hash {tempFileHash} doesn't match the expected remote hash {remoteFileHash}");
                    Logger.GetInstance().Error($"Failed to repair the game file \"{report.localFilePath}\"");

                }

                this.IntegrityCheckState = previousState;

            } catch (Exception) {

                this.IntegrityCheckState = previousState;
                throw;

            }

        }

        public async Task RepairGameFiles(CancellationToken token = default) => await this.RepairGameFiles(null, token);
        public async Task RepairGameFiles(ProgressReporter<ProgressReport>? progress, CancellationToken token = default) {

            try {

                List<GameFileIntegrityReport> mismatches = await this.CheckFilesIntegrity(progress);

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

                foreach (var report in mismatches) {

                    await this.RepairFile(report, progress, token);

                }

            } catch (Exception e) {

                throw new GameException($"Failed to repair game's files", e);
                
            }

        }

        public virtual async Task Start() {

            if (this.Wine == null)
                throw new GameException("Wine not initialized");

            if (this.State != GameState.READY)
                throw new ForbiddenGameStateException(this.State);

            GameState savedState = this.State;

            try {

                this.UpdateEntityState(GameState.RUNNING);

                await this.Wine.Execute("Starter.exe", new List<string> { 
                    $"\"{Path.Join(this.Settings.InstallationDirectory, $"{GameConstants.GAME_TITLE[this.Settings.Server]}.exe")}\""
                 });

            } finally {

                this.UpdateEntityState(savedState);

            }

        }

        private void UpdateEntityState(GameState newState) {

            Logger.GetInstance().Debug($"Updating game state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;
            this.OnStateUpdate.Invoke(this, newState);

        }

    }

}