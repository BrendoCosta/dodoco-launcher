using Dodoco.Core.Network;
using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Network.HTTP;
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

    public abstract class Game: IGame {

        public string InstallationDirectory { get; private set; }
        public GameServer GameServer { get; private set; }
        public GameState State { get; private set; }
        public Version Version { get; private set; }
        public IWine Wine { get; private set; }
        public Resource Resource { get; private set; }
        public Resource LatestResource { get; set; }
        public bool IsUpdating {
            get =>
                this.State == GameState.DOWNLOADING_UPDATE
                || this.State == GameState.EXTRACTING_UPDATE
                || this.State == GameState.PATCHING_FILES
                || this.State == GameState.REMOVING_DEPRECATED_FILES;
        }
        
        public event EventHandler<ProgressReport> OnCheckIntegrityProgress = delegate {};
        public event EventHandler<ProgressReport> OnDownloadProgress = delegate {};

        public Game(Version version, GameServer server, Resource resource, Resource latestResource, IWine wine, string InstallationDirectory, GameState state) {

            this.InstallationDirectory = InstallationDirectory;
            this.State = state;
            this.Version = version;
            if (!resource.IsSuccessfull()) throw new GameException("Got an invalid resource");
            this.Resource = resource;
            this.LatestResource = latestResource;
            this.Wine = wine;

        }

        public virtual void SetInstallationDirectory(string directory) => this.InstallationDirectory = directory;
        public virtual void SetVersion(Version version) => this.Version = version;

        public virtual string GetInstallationDirectory() => this.InstallationDirectory;
        public virtual GameState GetGameState() => this.State;
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
            ulong storageFreeBytes = Convert.ToUInt64(FileSystem.GetAvaliableStorageSpace(this.InstallationDirectory)); 
            
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

        public virtual async Task Update(ProgressReporter<ProgressReport>? progress = null) {

            if (this.State != GameState.WAITING_FOR_UPDATE)
                throw new ForbiddenGameStateException(this.State);

            if (this.LatestResource == null)
                throw new GameException($"Latest resource was not supplied");

            GameState previousState = this.State;

            try {

                Version remoteVersion = Version.Parse(this.LatestResource.data.game.latest.version);
                if (this.Version >= remoteVersion)
                    throw new GameException($"Game is already updated");

                Logger.GetInstance().Log($"Updating the game from version \"{this.Version.ToString()}\" to \"{remoteVersion.ToString()}\"...");

                string zipFileNamePattern = @$"(game_{this.Version.ToString().Replace(".", @"\.")}_{remoteVersion.ToString().Replace(".", @"\.")}_hdiff_(\w*)\.zip)";
                
                if (this.LatestResource.data.game.diffs.Exists(d => Regex.IsMatch(d.name, zipFileNamePattern))) {

                    Resource.Diff hdiffInfo = this.LatestResource.data.game.diffs.Find(d => Regex.IsMatch(d.name, zipFileNamePattern));
                    string zipFilePath = Path.Join(this.InstallationDirectory, hdiffInfo.name);

                    /*
                     * Downloads the update's zip file or skips
                     * its download if already in game's directory
                    */

                    this.UpdateState(GameState.DOWNLOADING_UPDATE);

                    ulong avaliableStorageSpace = Convert.ToUInt64(FileSystem.GetAvaliableStorageSpace(this.InstallationDirectory));
                    if (hdiffInfo.size >= avaliableStorageSpace) {

                        throw new GameException($"There is no enough storage space available to download the game update. This update requires {DataUnitFormatter.Format(hdiffInfo.size)} of storage space, but there is only {DataUnitFormatter.Format(avaliableStorageSpace)} avaliable");

                    }

                    if (File.Exists(zipFilePath) && new Hash(MD5.Create()).ComputeHash(zipFilePath).ToUpper() == hdiffInfo.md5.ToUpper()) {

                        Logger.GetInstance().Log($"Found the game's update's zip file already inside the game's installation directory, skipping the download");

                    } else {

                        try {

                            Logger.GetInstance().Log($"Downloading the game update...");
                            await Client.GetInstance().DownloadFileAsync(new Uri(hdiffInfo.path), zipFilePath, progress);
                            
                            Logger.GetInstance().Log($"Succesfully downloaded the game update");
                            Logger.GetInstance().Log($"Checking game's update's zip file's integrity...");

                            string zipFileHash = new Hash(MD5.Create()).ComputeHash(zipFilePath);

                            if (zipFileHash.ToUpper() == hdiffInfo.md5.ToUpper()) {

                                Logger.GetInstance().Log($"The downloaded file's hash match the expected remote hash");

                            } else {

                                throw new GameException($"The downloaded file's hash ({zipFileHash}) doesn't match the expected remote hash ({hdiffInfo.md5.ToUpper()})");

                            }

                        } catch (NetworkException e) {

                            throw new GameException($"Failed to update the game due to a network error", e);

                        }

                    }

                    /*
                     * Before apply the update, all game's files from the
                     * current game version must be upright,
                     * otherwise the hdiff patches will fail to apply and the
                     * game's installation will stay in an inconsistent state.
                    */

                    await this.RepairGameFiles(progress);

                    /*
                     * Decompress the update's zip file
                    */

                    this.UpdateState(GameState.EXTRACTING_UPDATE);
                    Logger.GetInstance().Log($"Decompressing the game update zip file...");
                    
                    try {

                        // overwriteFiles = true
                        ZipFile.ExtractToDirectory(zipFilePath, this.InstallationDirectory, true);

                    } catch (Exception e) {

                        throw new GameException($"Failed to decompress the game update zip file", e);

                    }
                    
                    Logger.GetInstance().Log($"Successfully finished decompressing the game's update's zip file");

                    /*
                     * Reads the "hdifffiles.txt" file and apply the patches for every referenced file
                    */

                    this.UpdateState(GameState.PATCHING_FILES);
                    GameHdiffFiles hdiffFilesHandler = new GameHdiffFiles(this.InstallationDirectory);
                    
                    Logger.GetInstance().Log($"Patching game's files...");

                    if (!hdiffFilesHandler.Exist()) {

                        throw new GameException($"Can't find the file \"{hdiffFilesHandler.FileName}\" inside the directory \"{hdiffFilesHandler.Directory}\"");

                    } else {

                        HDiffPatch patcher = new HDiffPatch();
                        int appliedPatchesCount = 0;
                        List<GameHdiffFilesEntry> patchesList = hdiffFilesHandler.Read();

                        foreach (GameHdiffFilesEntry entry in patchesList) {

                            string oldFilePath = Path.Join(this.InstallationDirectory, entry.remoteName);
                            string oldFileBackupPath = Path.Join(this.InstallationDirectory, entry.remoteName + ".bak");
                            string patchFilePath = Path.Join(this.InstallationDirectory, entry.remoteName + ".hdiff");
                            
                            try {

                                Logger.GetInstance().Log($"Patching the file \"{oldFilePath}\"...");

                                // Creates a backup of the file from current game's version
                                File.Copy(oldFilePath, oldFileBackupPath);

                                // Loads its patch file into the patcher
                                patcher.Initialize(patchFilePath);

                                // Patches the old file (it becomes the newer/updated file)
                                patcher.Patch(oldFileBackupPath, oldFilePath);

                                // Removes the backup file and the patch file
                                File.Delete(oldFileBackupPath);
                                File.Delete(patchFilePath);

                                Logger.GetInstance().Log($"Successfully patched the file \"{oldFilePath}\"");

                                appliedPatchesCount++;
                                progress?.Report(new ProgressReport {

                                    CompletionPercentage = ((double) appliedPatchesCount / (double) patchesList.Count) * 100,
                                    Message = oldFilePath

                                });

                            } catch (Exception e) {

                                Logger.GetInstance().Error($"Failed to patch the file \"{oldFilePath}\"", e);

                            }

                        }

                    }

                    /*
                     * Reads the "deletefiles.txt" file and deletes every referenced file
                    */

                    this.UpdateState(GameState.REMOVING_DEPRECATED_FILES);
                    GameDeleteFiles deleteFilesHandler = new GameDeleteFiles(this.InstallationDirectory);

                    if (!deleteFilesHandler.Exist()) {

                        throw new GameException($"Can't find the file \"{deleteFilesHandler.FileName}\" inside the directory \"{deleteFilesHandler.Directory}\"");

                    } else {

                        int removedFilesCount = 0;
                        List<string> filesToRemove = deleteFilesHandler.Read();

                        Logger.GetInstance().Log($"Removing deprecated files...");

                        foreach (string filePath in deleteFilesHandler.Read()) {

                            string fullPath = Path.Join(this.InstallationDirectory, filePath);

                            try {

                                Logger.GetInstance().Log($"Removing the file \"{fullPath}\"...");

                                File.Delete(fullPath);
                                
                                Logger.GetInstance().Log($"Successfully removed the file \"{fullPath}\"");

                                removedFilesCount++;
                                progress?.Report(new ProgressReport {
                                    
                                    CompletionPercentage = ((double) removedFilesCount / (double) filesToRemove.Count) * 100,
                                    Message = fullPath
                                    
                                });

                            } catch (Exception e) {

                                Logger.GetInstance().Error($"Failed to remove the file \"{fullPath}\"", e);

                            }

                        }

                        Logger.GetInstance().Log($"Finished removing deprecated files");

                    }

                } else {

                    throw new GameException($"Can't find a diff object whose name matchs the string pattern \"{zipFileNamePattern}\"");

                }

                Logger.GetInstance().Log($"Finished game update");
            
            } catch (CoreException e) {

                this.UpdateState(previousState);
                throw e;

            }

        }

        public virtual async Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(CancellationToken token = default) => await this.CheckFilesIntegrity(null, token);
        public virtual async Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(ProgressReporter<ProgressReport>? progress, CancellationToken token = default) {

            GameState savedState = this.State;
            this.UpdateState(GameState.CHECKING_INTEGRITY);
            
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
                    FileInfo file = new FileInfo(Path.Join(this.InstallationDirectory, currentEntry.remoteName));
                    Logger.GetInstance().Log($"Checking the file \"{file.FullName}\"...");
                    
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
                                remoteFileSize = currentEntry.fileSize

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
                            remoteFileSize = currentEntry.fileSize

                        });

                    }

                    ProgressReport report = new ProgressReport {

                        CompletionPercentage = ((double) i / (double) entries.Count) * 100.0D,
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

            this.UpdateState(savedState);
            return mismatches;

        }

        public async Task RepairFile(GameFileIntegrityReport report, CancellationToken token = default) => await this.RepairFile(report, null, token);
        public async Task RepairFile(GameFileIntegrityReport report, ProgressReporter<ProgressReport>? progress, CancellationToken token = default) {

            GameState savedState = this.State;
            this.UpdateState(GameState.REPAIRING_FILES);

            Logger.GetInstance().Log($"Trying to repair the game file \"{report.localFilePath}\"...");

            FileInfo localFile = new FileInfo(Path.Join(this.InstallationDirectory, report.remoteFilePath));
            FileInfo tempFile = new FileInfo(Path.Join(this.InstallationDirectory, Path.GetDirectoryName(report.remoteFilePath), Path.GetFileName(report.localFilePath) + ".temp"));
            Uri fileRemoteUrl = new Uri(UrlCombine.Combine(this.Resource.data.game.latest.decompressed_path.ToString(), report.remoteFilePath));

            try {

                Logger.GetInstance().Log($"Downloading a copy of the remote file to \"{tempFile.FullName}\"");
                
                await Client.GetInstance().DownloadFileAsync(fileRemoteUrl, tempFile.FullName, progress);
                
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

            } catch (NetworkException e) {

                throw new GameException($"Failed to repair the game file \"{report.localFilePath}\" due to a network error", e);

            }

            this.UpdateState(savedState);
            return;

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

                    await this.RepairFile(report);

                }

            } catch (Exception e) {

                throw new GameException($"Failed to repair game's files", e);
                
            }

        }

        private void UpdateState(GameState newState) {

            Logger.GetInstance().Debug($"Updating game state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;

        }

    }

}