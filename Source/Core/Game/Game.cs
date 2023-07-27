using Dodoco.Core.Network;
using Dodoco.Core.Network.Api.Company;
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

    public class Game: IGame {

        public GameSettings Settings { get; set; }
        public GameServer GameServer { get; protected set; }
        public bool IsInstalled { get => GameInstallationManager.CheckGameInstallation(this.Settings.InstallationDirectory, this.Settings.Server); }
        public GameState State { get; private set; } = GameState.READY;
        public IWine? Wine { get; set; }
        public Resource Resource { get; private set; }
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

        public Game(GameSettings settings, Resource resource) {

            this.Settings = settings;
            this.Resource = resource;

        }

        public virtual async Task<bool> IsUpdateAvaliable() {

            Resource latestResource = await this.apiFactory.FetchLauncherResource();
            return Version.Parse(latestResource.data.game.latest.version) > GameInstallationManager.SearchForGameVersion(this.Settings.InstallationDirectory, this.Settings.Server);

        }

        public virtual async Task Download(ProgressReporter<ProgressReport>? progress, CancellationToken token = default) {

            GameState savedState = this.State;

            try {

                if (this.IsInstalled)
                    throw new GameException("Game is already installed");
            
                Logger.GetInstance().Log($"Starting game download...");

                Resource latestResource = await this.apiFactory.FetchLauncherResource();
                if (!latestResource.IsSuccessfull())
                    throw new GameException($"Invalid resource");
                
                Logger.GetInstance().Log($"Downloading game's segments...");

                double totalBytesTransferred = 0.0D;

                for (int i = 0; i < latestResource.data.game.latest.segments.Count; i++) {

                    Resource.Segment segment = latestResource.data.game.latest.segments[i];
                    string segmentFileName = segment.path.Split("/").Last();

                    if (!Directory.Exists(this.Settings.InstallationDirectory))
                        Directory.CreateDirectory(this.Settings.InstallationDirectory);

                    if (File.Exists(Path.Join(this.Settings.InstallationDirectory, segmentFileName))) {

                        this.UpdateState(GameState.RECOVERING_DOWNLOADED_SEGMENTS);

                        ProgressReport report = new ProgressReport {
                            CompletionPercentage = (((double) i + 1) / (double) latestResource.data.game.latest.segments.Count) * 100.0D,
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

                    this.UpdateState(GameState.DOWNLOADING);

                    ProgressReporter<ProgressReport> segmentProgress = new ProgressReporter<ProgressReport>();
                    segmentProgress.ProgressChanged += (object? s, ProgressReport e) => {

                        totalBytesTransferred += e.BytesTransferred;

                        ProgressReport generalProgress = new ProgressReport {

                            CompletionPercentage = (totalBytesTransferred / (double) latestResource.data.game.latest.package_size) * 100.0D,
                            BytesPerSecond = e.BytesPerSecond,
                            TotalBytesTransferred = totalBytesTransferred,
                            EstimatedRemainingTime = TimeSpan.FromSeconds((double) (latestResource.data.game.latest.package_size - (double) totalBytesTransferred) / e.BytesPerSecond),
                            Message = e.Message

                        };

                        progress?.Report(generalProgress);

                    };

                    Logger.GetInstance().Log($"Downloading the game segment \"{segmentFileName}\" (segment no. {i+1} of {latestResource.data.game.latest.segments.Count})");
                    
                    Logger.GetInstance().Log($"Checking storage device available space...");
                    long storageFreeBytes = FileSystem.GetAvaliableStorageSpace(this.Settings.InstallationDirectory); 
                    if (segment.package_size > storageFreeBytes)
                        throw new GameException($"There is no enough storage space available to download the game's segment {i+1}. The download's process requires {DataUnitFormatter.Format(segment.package_size)} of storage space, but there is only {DataUnitFormatter.Format(storageFreeBytes)} avaliable. Try freeing up storage space and restart the download; already downloaded files will not need to be redownloaded");

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
                Resource.Segment firstSegment = latestResource.data.game.latest.segments.First();

                using (Stream fileStream = File.OpenRead(Path.Join(this.Settings.InstallationDirectory, firstSegment.path.Split("/").Last()))) {

                    const int ZIPFILE_MAGIC_NUMBER_BYTE_SIZE = 4;
                    byte[] zipFileMagicNumber = new byte[ZIPFILE_MAGIC_NUMBER_BYTE_SIZE] { 0x50, 0x4B, 0x03, 0x04 };
                    byte[] buffer = new byte[ZIPFILE_MAGIC_NUMBER_BYTE_SIZE] { 0x00, 0x00, 0x00, 0x00 };
                    
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.ReadExactly(buffer, 0, ZIPFILE_MAGIC_NUMBER_BYTE_SIZE);
                    
                    if (!buffer.SequenceEqual(zipFileMagicNumber))
                        throw new GameException("The first game's segment is not a zip file");

                }

                this.UpdateState(GameState.EXTRACTING_DOWNLOADED_SEGMENTS);
                Logger.GetInstance().Log($"Extracting downloaded game's segments...");

                /* Creates a single and memory-contiguous stream composed by the filestreams of
                 * each segment file. This eliminates the need of joining all segments into
                 * a single, big zip file before extracting it, thereby saving disk space and
                 * speeding up the whole process. */

                using (MultiStream.Lib.MultiStream segmentsMultiStream = new MultiStream.Lib.MultiStream(latestResource.data.game.latest.segments.Select(s => File.OpenRead(Path.Join(this.Settings.InstallationDirectory, s.path.Split("/").Last()))))) {

                    ZipArchive zipArchive = new ZipArchive(segmentsMultiStream, ZipArchiveMode.Read);
                   
                    long zipArchiveFullLengthBytes = zipArchive.Entries.Select(e => e.Length).Sum();
                    long totalBytesRead = 0;

                    List<double> remainingTimeGuesses = new List<double>();
                    Stopwatch watch = new Stopwatch();

                    long storageFreeBytes = FileSystem.GetAvaliableStorageSpace(this.Settings.InstallationDirectory);
                    if (zipArchiveFullLengthBytes > storageFreeBytes)
                        throw new GameException($"There is no enough storage space available to extract the game's segments. This process requires {DataUnitFormatter.Format(zipArchiveFullLengthBytes)} of storage space, but there is only {DataUnitFormatter.Format(storageFreeBytes)} avaliable. Try freeing up storage space and restart the download; already downloaded files will not need to be redownloaded");
                    
                    for (int i = 0; i < zipArchive.Entries.Count; i++) {

                        ZipArchiveEntry entry = zipArchive.Entries[i];

                        if (entry.FullName.EndsWith("/") && string.IsNullOrWhiteSpace(entry.Name)) {

                            /* The ZipArchiveEntry.ExtractToFile method doesn't creates zip archive's internal
                             * directories due some unknown reason, so we need to create them otherwise an
                             * UnauthorizedAccessException will be raised up. */

                            Directory.CreateDirectory(Path.Join(this.Settings.InstallationDirectory, entry.FullName));
                            continue;

                        }

                        string entryDestinationFullPath = Path.Join(this.Settings.InstallationDirectory, entry.FullName);

                        watch.Restart();
                        entry.ExtractToFile(entryDestinationFullPath, true);
                        watch.Stop();

                        totalBytesRead += entry.Length;
                        double bytesPerSecond = (double) entry.Length / (double) watch.Elapsed.TotalSeconds;
                        double currentRemainingTimeEstimation = ((double) zipArchiveFullLengthBytes - (double) totalBytesRead) / (double) bytesPerSecond;
                        remainingTimeGuesses.Add(currentRemainingTimeEstimation);

                        ProgressReport report = new ProgressReport {
                            BytesTransferred = entry.Length,
                            BytesPerSecond = bytesPerSecond,
                            CompletionPercentage = (((double) i + 1) / (double) zipArchive.Entries.Count) * 100.0D,
                            EstimatedRemainingTime = TimeSpan.FromSeconds(remainingTimeGuesses.Count >= 2 ? remainingTimeGuesses.Average() : currentRemainingTimeEstimation),
                            Message = entryDestinationFullPath,
                            TotalBytesTransferred = totalBytesRead
                        };

                        progress?.Report(report);
                        if (remainingTimeGuesses.Count > 9)
                            remainingTimeGuesses.Clear();

                    }

                    watch.Stop();

                }

                Logger.GetInstance().Log($"Sucessfully extracted downloaded game's segments...");

                Logger.GetInstance().Log($"Successfully downloaded the game");
                this.AfterGameDownload.Invoke(this, EventArgs.Empty);

            } catch (NetworkException e) {

                throw new GameException($"Failed to download the game due to a network error", e);

            } finally {

                this.UpdateState(savedState);

            }

        }

        public virtual async Task Update(ProgressReporter<ProgressReport>? progress = null) {

            if (!this.IsInstalled)
                throw new GameException("Game is not installed");

            if (!await this.IsUpdateAvaliable())
                throw new GameException("Game is already updated");
                
            Resource latestResource = await this.apiFactory.FetchLauncherResource();

            if (!latestResource.IsSuccessfull())
                throw new GameException("Invalid resource");

            GameState previousState = this.State;

            try {

                Version remoteVersion = Version.Parse(latestResource.data.game.latest.version);
                if (this.Version >= remoteVersion)
                    throw new GameException($"Game is already updated");

                Logger.GetInstance().Log($"Updating the game from version \"{this.Version.ToString()}\" to \"{remoteVersion.ToString()}\"...");

                string zipFileNamePattern = @$"(game_{this.Version.ToString().Replace(".", @"\.")}_{remoteVersion.ToString().Replace(".", @"\.")}_hdiff_(\w*)\.zip)";
                
                if (latestResource.data.game.diffs.Exists(d => Regex.IsMatch(d.name, zipFileNamePattern))) {

                    Resource.Diff hdiffInfo = latestResource.data.game.diffs.Find(d => Regex.IsMatch(d.name, zipFileNamePattern));
                    string zipFilePath = Path.Join(this.Settings.InstallationDirectory, hdiffInfo.name);

                    /*
                     * Downloads the update's zip file or skips
                     * its download if already in game's directory
                    */

                    this.UpdateState(GameState.DOWNLOADING_UPDATE);

                    long avaliableStorageSpace = FileSystem.GetAvaliableStorageSpace(this.Settings.InstallationDirectory);
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
                        ZipFile.ExtractToDirectory(zipFilePath, this.Settings.InstallationDirectory, true);

                    } catch (Exception e) {

                        throw new GameException($"Failed to decompress the game update zip file", e);

                    }
                    
                    Logger.GetInstance().Log($"Successfully finished decompressing the game's update's zip file");

                    /*
                     * Reads the "hdifffiles.txt" file and apply the patches for every referenced file
                    */

                    this.UpdateState(GameState.PATCHING_FILES);
                    GameHdiffFiles hdiffFilesHandler = new GameHdiffFiles(this.Settings.InstallationDirectory);
                    
                    Logger.GetInstance().Log($"Patching game's files...");

                    if (!hdiffFilesHandler.Exist()) {

                        throw new GameException($"Can't find the file \"{hdiffFilesHandler.FileName}\" inside the directory \"{hdiffFilesHandler.Directory}\"");

                    } else {

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
                    GameDeleteFiles deleteFilesHandler = new GameDeleteFiles(this.Settings.InstallationDirectory);

                    if (!deleteFilesHandler.Exist()) {

                        throw new GameException($"Can't find the file \"{deleteFilesHandler.FileName}\" inside the directory \"{deleteFilesHandler.Directory}\"");

                    } else {

                        int removedFilesCount = 0;
                        List<string> filesToRemove = deleteFilesHandler.Read();

                        Logger.GetInstance().Log($"Removing deprecated files...");

                        foreach (string filePath in deleteFilesHandler.Read()) {

                            string fullPath = Path.Join(this.Settings.InstallationDirectory, filePath);

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
                this.AfterGameUpdate.Invoke(this, EventArgs.Empty);
            
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
                    FileInfo file = new FileInfo(Path.Join(this.Settings.InstallationDirectory, currentEntry.remoteName));
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

            try {

                Logger.GetInstance().Log($"Trying to repair the game file \"{report.localFilePath}\"...");

                FileInfo localFile = new FileInfo(Path.Join(this.Settings.InstallationDirectory, report.remoteFilePath));
                FileInfo tempFile = new FileInfo(Path.Join(this.Settings.InstallationDirectory, Path.GetDirectoryName(report.remoteFilePath), Path.GetFileName(report.localFilePath) + ".temp"));
                Uri fileRemoteUrl = new Uri(UrlCombine.Combine(this.Resource.data.game.latest.decompressed_path.ToString(), report.remoteFilePath));

                Logger.GetInstance().Log($"Downloading a copy of the remote file to \"{tempFile.FullName}\"");
                
                await Client.GetInstance().DownloadFileAsync(fileRemoteUrl, tempFile.FullName, progress, token);
                
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

            } finally {

                this.UpdateState(savedState);

            }

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

                this.UpdateState(GameState.RUNNING);

                await this.Wine.Execute("Starter.exe", new List<string> { 
                    $"\"{Path.Join(this.Settings.InstallationDirectory, $"{GameConstants.GAME_TITLE[this.Settings.Server]}.exe")}\""
                 });

            } finally {

                this.UpdateState(savedState);

            }

        }

        private void UpdateState(GameState newState) {

            Logger.GetInstance().Debug($"Updating game state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;
            this.OnStateUpdate.Invoke(this, newState);

        }

    }

}