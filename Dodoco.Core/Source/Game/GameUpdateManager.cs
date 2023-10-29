namespace Dodoco.Core.Game;

using Dodoco.Core.Extension;
using Dodoco.Core.Network;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Util.FileSystem;
using Dodoco.Core.Util.Hash;
using Dodoco.Core.Util.Log;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public class GameUpdateManager: IGameUpdateManager {

    private IGameEx _Game;
    
    private GameUpdateManagerState _State;
    public GameUpdateManagerState State {
        get => this._State;
        protected set {
            Logger.GetInstance().Debug($"Updating {nameof(GameUpdateManagerState)} from {this._State.ToString()} to {value.ToString()}");
            this._State = value;
        }
    }

    /// <inheritdoc />
    public event EventHandler<GameUpdateManagerState> OnStateUpdate = delegate {};

    public GameUpdateManager(IGameEx game) => this._Game = game;

    /// <summary>
    /// Reads the game's update package <see cref="T:Dodoco.Core.Game.GameHdiffFiles"/> file
    /// and applies all hdiff patches for the listed files.
    /// </summary>
    protected virtual async void ApplyGameUpdatePackagePatches(ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

        GameHdiffFiles hdiffFilesHandler = new GameHdiffFiles(this._Game.Settings.InstallationDirectory);

        if (!hdiffFilesHandler.Exist())
            return;

        GameUpdateManagerState previousState = this.State;

        try {

            /*
             * Reads the "hdifffiles.txt" file and apply the patches for every referenced file
            */

            this.State = GameUpdateManagerState.APPLYING_UPDATE_PACKAGE;
            Logger.GetInstance().Log($"Applying game's update package patches...");

            GameHDiffPatcher patcher = new GameHDiffPatcher();
            int appliedPatchesCount = 0;
            List<GameHdiffFilesEntry> patchesList = hdiffFilesHandler.Read();

            await Parallel.ForEachAsync(patchesList, async (entry, token) => {

                string oldFilePath = Path.Join(this._Game.Settings.InstallationDirectory, entry.remoteName);
                string oldFileBackupPath = Path.Join(this._Game.Settings.InstallationDirectory, entry.remoteName + ".bak");
                string patchFilePath = Path.Join(this._Game.Settings.InstallationDirectory, entry.remoteName + ".hdiff");
                
                try {

                    Logger.GetInstance().Log($"Patching the file \"{oldFilePath}\"...");

                    // Creates a backup of the file from current game's version
                    File.Copy(oldFilePath, oldFileBackupPath);

                    // Patches the old file (it becomes the newer/updated file)
                    await patcher.Patch(patchFilePath, oldFileBackupPath, oldFilePath);

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

                    Logger.GetInstance().Error($"Error while patching the file \"{oldFilePath}\"", e);

                }

            });

            Logger.GetInstance().Log($"Sucessfully applied game's update package patches");
            this.State = previousState;
            return;

        } catch (CoreException e) {

            throw new GameException($"Failed to apply the game's update package patches", e);

        } finally {

            this.State = previousState;

        }

    }

    /// <summary>
    /// Downloads the <see cref="T:Dodoco.Core.Protocol.Company.Launcher.Resource.ResourceDiff"/> update zip package to the game installation directory.
    /// If the package file already exists in the installation directory and its checksum
    /// matches that reported by the server, the file will not be download again; if the
    /// checksum doesn't matches, the package file will be overwritten.
    /// </summary>
    protected virtual async Task DownloadUpdatePackageAsync(ResourceDiff diff, ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

        GameUpdateManagerState previousState = this.State;

        try {

            string packageFileFullPath = Path.Join(this._Game.Settings.InstallationDirectory, diff.name);

            /*
             * Downloads the update's package or skips
             * its download if already in game's directory
            */

            this.State = GameUpdateManagerState.DOWNLOADING_UPDATE_PACKAGE;

            if (File.Exists(packageFileFullPath) && new Hash(MD5.Create()).ComputeHash(packageFileFullPath).ToUpper() == diff.md5.ToUpper()) {

                Logger.GetInstance().Log($"Found the game's update's package already inside the game's installation directory, skipping the download");

            } else {

                try {

                    long availableStorageSpace = FileSystem.GetAvailableStorageSpace(this._Game.Settings.InstallationDirectory);
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
            this.State = previousState;
            return;

        } catch (CoreException e) {

            throw new GameException($"Failed to download the game's update package", e);

        } finally {

            this.State = previousState;

        }

    }

    /// <inheritdoc />
    public virtual async Task<ResourceGame?> GetGamePreUpdateAsync() {

        ResourceResponse latestResource = await this._Game.GetApiFactory().FetchLauncherResource();
        latestResource.EnsureSuccessStatusCode();
        return latestResource.data.pre_download_game;

    }

    /// <inheritdoc />
    public virtual async Task<ResourceGame?> GetGameUpdateAsync() {

        if (!this._Game.CheckGameInstallation())
            return null;

        ResourceResponse latestResource = await this._Game.GetApiFactory().FetchLauncherResource();
        latestResource.EnsureSuccessStatusCode();

        if (Version.Parse(latestResource.data.game.latest.version) > await this._Game.GetGameVersionAsync())
            return latestResource.data.game;

        return null;

    }

    protected virtual string GetGameUpdatePackageFilenamePattern(Version currentVersion, Version targetVersion) {

        return @$"(game_{currentVersion.ToString().Replace(".", @"\.")}_{targetVersion.ToString().Replace(".", @"\.")}_hdiff_(\w*)\.zip)";

    }

    /// <inheritdoc />
    public virtual async Task<bool> IsGamePreUpdateDownloadedAsync() {

        ResourceGame? gameResource = await this.GetGamePreUpdateAsync();
        Version currentVersion = await this._Game.GetGameVersionAsync();

        if (gameResource != null) {

            Version remoteVersion = Version.Parse(((ResourceGame) gameResource).latest.version);
            string packageFilenamePattern = @$"(game_{currentVersion.ToString().Replace(".", @"\.")}_{remoteVersion.ToString().Replace(".", @"\.")}_hdiff_(\w*)\.zip)";

            if (Directory.EnumerateFiles(this._Game.Settings.InstallationDirectory).ToList().Exists(someFile => Regex.IsMatch(Path.GetFileName(someFile), packageFilenamePattern)))
                return true;

        }

        return false;

    }

    public virtual async Task PreUpdateGameAsync(ProgressReporter<ProgressReport>? reporter = null, CancellationToken token = default) {

        if (!this._Game.CheckGameInstallation())
            throw new GameException("Game is not installed");

        GameUpdateManagerState previousState = this.State;

        try {

            ResourceGame gameResource = await this.GetGamePreUpdateAsync() ?? throw new GameException("Game pre-update is not available");
            
            Version currentVersion = await this._Game.GetGameVersionAsync();
            Version remoteVersion = Version.Parse(gameResource.latest.version);
            string packageFilenamePattern = this.GetGameUpdatePackageFilenamePattern(currentVersion, remoteVersion);

            Logger.GetInstance().Log($"Pre-updating the game to version {remoteVersion.ToString()}...");

            if (gameResource.diffs.Exists(d => Regex.IsMatch(d.name, packageFilenamePattern))) {

                ResourceDiff diff = gameResource.diffs.Find(d => Regex.IsMatch(d.name, packageFilenamePattern));
                string packageFileFullPath = Path.Join(this._Game.Settings.InstallationDirectory, diff.name);
                await this.DownloadUpdatePackageAsync(diff, reporter, token);

            } else {

                throw new GameException($"Can't find a diff object whose name matchs the string pattern \"{packageFilenamePattern}\"");

            }
            
            Logger.GetInstance().Log($"Sucessfully pre-updated the game to version {remoteVersion.ToString()}");
            this.State = previousState;
            return;

        } catch (CoreException e) {

            throw new GameException($"Failed to pre-update the game", e);

        } finally {

            this.State = previousState;

        }

    }

    /// <summary>
    /// Remove all game's deprecated files listed in the <see cref="T:Dodoco.Core.Game.GameDeleteFiles"/> file.
    /// </summary>
    protected virtual void RemoveGameDeprecatedFiles(ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

        GameDeleteFiles deleteFilesHandler = new GameDeleteFiles(this._Game.Settings.InstallationDirectory);

        if (!deleteFilesHandler.Exist())
            return;

        GameUpdateManagerState previousState = this.State;

        try {

            /*
             * Reads the "deletefiles.txt" file and deletes every listed file
            */

            this.State = GameUpdateManagerState.REMOVING_DEPRECATED_FILES;
            Logger.GetInstance().Log($"Removing game's deprecated files...");

            int removedFilesCount = 0;
            List<string> filesToRemove = deleteFilesHandler.Read();

            Parallel.ForEach(filesToRemove, filePath => {

                string fullPath = Path.Join(this._Game.Settings.InstallationDirectory, filePath);

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

            });

            Logger.GetInstance().Log($"Sucessfully removed game's deprecated files");
            this.State = previousState;
            return;

        } catch (CoreException e) {

            throw new GameException($"Failed to remove game's deprecated files", e);

        } finally {

            this.State = previousState;

        }

    }

    /// <summary>
    /// Unzips the downloaded game's update package to the game's installation directory,
    /// replacing its files with those from the update.
    /// </summary>
    protected virtual void UnzipGameUpdatePackage(string packagePath, ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

        if (string.IsNullOrWhiteSpace(packagePath))
            throw new GameException("Empty game's update package path");

        if (!File.Exists(packagePath))
            throw new GameException("The game's update package is missing");

        GameUpdateManagerState previousState = this.State;

        try {

            /*
             * Removes old update package's files
            */

            GameHdiffFiles hdiffFilesHandler = new GameHdiffFiles(this._Game.Settings.InstallationDirectory);
            GameDeleteFiles deleteFilesHandler = new GameDeleteFiles(this._Game.Settings.InstallationDirectory);

            if (hdiffFilesHandler.Exist())
                hdiffFilesHandler.Delete();
            
            if (deleteFilesHandler.Exist())
                deleteFilesHandler.Delete();

            using (FileStream zipFileStream = File.OpenRead(packagePath)) {

                /*
                 * Unzips the update package
                */

                this.State = GameUpdateManagerState.UNZIPPING_UPDATE_PACKAGE;
                Logger.GetInstance().Log($"Unzipping the game's update package...");
                
                try {

                    using(ZipArchive zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read)) {

                        // overwrite files = true
                        zipArchive.ExtractToDirectory(this._Game.Settings.InstallationDirectory, true, reporter);

                    }
                    

                } catch (Exception e) {

                    throw new GameException($"Failed to unzip the game's update package", e);

                }
                
                Logger.GetInstance().Log($"Successfully finished unzipping the game's update package");
                this.State = previousState;
                return;

            }

        } catch (CoreException e) {

            throw new GameException($"Failed to unzip the game's update package", e);

        } finally {

            this.State = previousState;

        }

    }

    /// <inheritdoc />
    public virtual async Task UpdateGameAsync(IGameIntegrityManager integrityManager, ProgressReporter<ProgressReport>? reporter = null, CancellationToken token = default) {

        if (!this._Game.CheckGameInstallation())
            throw new GameException("Game is not installed");

        GameUpdateManagerState previousState = this.State;

        try {

            ResourceGame gameResource = await this.GetGameUpdateAsync() ?? throw new GameException("Game update is not available");
            
            Version currentVersion = await this._Game.GetGameVersionAsync();
            Version remoteVersion = Version.Parse(gameResource.latest.version);
            string packageFilenamePattern = this.GetGameUpdatePackageFilenamePattern(currentVersion, remoteVersion);

            Logger.GetInstance().Log($"Updating the game to version {remoteVersion.ToString()}...");

            if (gameResource.diffs.Exists(d => Regex.IsMatch(d.name, packageFilenamePattern))) {

                Task repairTask = new Task(async () => {
                
                    await integrityManager.RepairInstallationAsync(await integrityManager.GetInstallationIntegrityReportAsync(reporter, token), reporter, token);
                
                });
                
                repairTask.Start();

                ResourceDiff diff = gameResource.diffs.Find(d => Regex.IsMatch(d.name, packageFilenamePattern));
                string packageFileFullPath = Path.Join(this._Game.Settings.InstallationDirectory, diff.name);
                await this.DownloadUpdatePackageAsync(diff, reporter, token);
                
                await repairTask;
                
                this.UnzipGameUpdatePackage(packageFileFullPath, reporter, token);
                this.ApplyGameUpdatePackagePatches(reporter, token);
                this.RemoveGameDeprecatedFiles(reporter, token);

            } else {

                throw new GameException($"Can't find a diff object whose name matchs the string pattern \"{packageFilenamePattern}\"");

            }
            
            Logger.GetInstance().Log($"Sucessfully updated the game to version {remoteVersion.ToString()}");
            this.State = previousState;
            return;

        } catch (CoreException e) {

            throw new GameException($"Failed to update the game", e);

        } finally {

            this.State = previousState;

        }

    }

}