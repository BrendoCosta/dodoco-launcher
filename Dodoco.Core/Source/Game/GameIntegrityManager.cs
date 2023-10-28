namespace Dodoco.Core.Game;

using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Serialization.Json;
using Dodoco.Core.Util.FileSystem;
using Dodoco.Core.Util.Hash;
using Dodoco.Core.Util.Log;

using UrlCombineLib;
using System.Diagnostics;
using System.Security.Cryptography;

public class GameIntegrityManager: IGameIntegrityManager {

    private IGameEx _Game;
    
    private GameIntegrityManagerState _State;
    public GameIntegrityManagerState State {
        get => this._State;
        protected set {
            Logger.GetInstance().Debug($"Updating {nameof(GameIntegrityManagerState)} from {this._State.ToString()} to {value.ToString()}");
            this._State = value;
        }
    }

    public GameIntegrityManager(IGameEx game) => this._Game = game;

    /// <inheritdoc />
    public event EventHandler<GameIntegrityManagerState> OnStateUpdate = delegate {};
    
    /// <inheritdoc />
    public virtual async Task<List<GameFileIntegrityReportEx>> GetInstallationIntegrityReportAsync(CancellationToken token = default) => await this.GetInstallationIntegrityReportAsync(null, token);
    
    /// <inheritdoc />
    public virtual async Task<List<GameFileIntegrityReportEx>> GetInstallationIntegrityReportAsync(ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

        GameIntegrityManagerState previousState = this.State;

        try {

            this.State = GameIntegrityManagerState.CHECKING_INTEGRITY;
            Logger.GetInstance().Log($"Starting game integrity check...");

            var entries = new List<GamePkgVersionEntry>();
            var mismatches = new List<GameFileIntegrityReportEx>();
            double pkgVersionTotalPackageSize = 0;
            double totalBytesRead = 0;
            List<double> estimatedRemainingTime = new List<double>();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            entries = await this.GetPkgVersionAsync();

            pkgVersionTotalPackageSize = entries.Select(e => e.fileSize).Sum();
            Logger.GetInstance().Log($"pkg_version total size is {DataUnitFormatter.Format(pkgVersionTotalPackageSize)}");
            
            for (int i = 0; i < entries.Count; i++) {

                GamePkgVersionEntry currentEntry = entries[i];
                FileInfo file = new FileInfo(Path.Join(this._Game.Settings.InstallationDirectory, currentEntry.remoteName));
                
                if (file.Exists) {

                    string localHash = new Hash(MD5.Create()).ComputeHash(file.FullName);

                    if (localHash.ToUpper() != currentEntry.md5.ToUpper()) {

                        mismatches.Add(new GameFileIntegrityReportEx {

                            State = GameFileIntegrityState.CORRUPTED,
                            Path = currentEntry.remoteName,
                            LocalChecksum = localHash.ToUpper(),
                            LocalSize = file.Length,
                            RemoteChecksum = currentEntry.md5.ToUpper(),
                            RemoteSize = currentEntry.fileSize

                        });

                    }

                    totalBytesRead += file.Length;
                    estimatedRemainingTime.Add(((pkgVersionTotalPackageSize - totalBytesRead) * watch.Elapsed.TotalSeconds) / totalBytesRead);

                } else {

                    mismatches.Add(new GameFileIntegrityReportEx {

                        State = GameFileIntegrityState.MISSING,
                        Path = currentEntry.remoteName,
                        LocalChecksum = string.Empty,
                        LocalSize = 0,
                        RemoteChecksum = currentEntry.md5.ToUpper(),
                        RemoteSize = currentEntry.fileSize

                    });

                }

                ProgressReport report = new ProgressReport {
                    Done = i + 1,
                    Total = entries.Count,
                    EstimatedRemainingTime = TimeSpan.FromSeconds(estimatedRemainingTime.Count >= 2 ? estimatedRemainingTime.Average() : 1),
                    Message = file.FullName
                };

                reporter?.Report(report);

                if (estimatedRemainingTime.Count > 9) estimatedRemainingTime.Clear();

            }

            watch.Stop();

            Logger.GetInstance().Log($"Successfully finished game integrity check ({DataUnitFormatter.Format(totalBytesRead)} read with {mismatches.Count} mismatches found)");
            this.State = previousState;

            foreach (var report in mismatches) {

                string message = $"Mismatch: the local file \"{Path.Join(this._Game.Settings.InstallationDirectory, report.Path)}\"";

                switch (report.State) {

                    case GameFileIntegrityState.CORRUPTED:
                        Logger.GetInstance().Warning($"{message} MD5 checksum ({report.LocalChecksum}) doesn't match the expected remote checksum ({report.RemoteChecksum})");
                        break;
                    
                    case GameFileIntegrityState.MISSING:
                        Logger.GetInstance().Warning($"{message} is missing");
                        break;

                }

            }

            return mismatches;

        } catch (CoreException e) {

            throw new GameException("Failed to check game installation integrity", e);

        } finally {

            this.State = previousState;

        }

    }
    
    /// <inheritdoc />
    public virtual async Task<List<GameFileIntegrityReportEx>> RepairInstallationAsync(List<GameFileIntegrityReportEx> reports, CancellationToken token = default) => await this.RepairInstallationAsync(reports, null, token);
    
    /// <inheritdoc />
    public virtual async Task<List<GameFileIntegrityReportEx>> RepairInstallationAsync(List<GameFileIntegrityReportEx> reports, ProgressReporter<ProgressReport>? reporter, CancellationToken token = default) {

        GameIntegrityManagerState previousState = this.State;
        List<GameFileIntegrityReportEx> fixedList = new List<GameFileIntegrityReportEx>();

        try {

            for (int i = 0; i < reports.Count; i++) {

                this.State = GameIntegrityManagerState.DOWNLOADING_FILE;
                Logger.GetInstance().Log($"Downloading the file \"{reports[i].Path}\" from the remote server...");
                string localFilePath = Path.Join(this._Game.Settings.InstallationDirectory, reports[i].Path);
                string tempFilePath = localFilePath + ".temp";
                Uri remoteFileUrl = new Uri(UrlCombine.Combine((await this._Game.GetResourceAsync()).data.game.latest.decompressed_path, reports[i].Path));

                await Client.GetInstance().DownloadFileAsync(remoteFileUrl, tempFilePath, reporter, token);
                
                this.State = GameIntegrityManagerState.REPAIRING_FILE;

                Logger.GetInstance().Log($"Computing the checksum of the downloaded file...");
                string tempFileChecksum = new Hash(MD5.Create()).ComputeHash(tempFilePath);
                string remoteFileChecksum = reports[i].RemoteChecksum;

                if (tempFileChecksum.ToUpper() == remoteFileChecksum.ToUpper()) {

                    Logger.GetInstance().Log($"The downloaded file's checksum match the expected remote hash");

                    File.Delete(localFilePath);
                    File.Move(tempFilePath, localFilePath);

                    Logger.GetInstance().Log($"Successfully repaired the game file \"{localFilePath}\"");
                    
                    fixedList.Add(new GameFileIntegrityReportEx {

                        State = GameFileIntegrityState.OK,
                        Path = reports[i].Path,
                        LocalChecksum = reports[i].RemoteChecksum.ToUpper(),
                        LocalSize = reports[i].RemoteSize,
                        RemoteChecksum = reports[i].RemoteChecksum.ToUpper(),
                        RemoteSize = reports[i].RemoteSize

                    });

                } else {

                    Logger.GetInstance().Error($"The downloaded file's checksum {tempFileChecksum.ToUpper()} doesn't match the expected remote checksum {reports[i].RemoteChecksum.ToUpper()}");
                    Logger.GetInstance().Error($"Failed to repair the game file \"{localFilePath}\"");

                    File.Delete(tempFilePath);

                }

                reporter?.Report(new ProgressReport {

                    Done = i + 1,
                    Total = reports.Count,
                    Message = reports[i].Path

                });

            }

            this.State = previousState;
            return fixedList;

        } catch (CoreException e) {

            throw new GameException($"Failed to repair game installation", e);

        } finally {

            this.State = previousState;

        }

    }

    /// <inheritdoc />
    public virtual async Task<List<GamePkgVersionEntry>> GetPkgVersionAsync() {

        Uri pkgVersionRemoteUrl = new Uri(UrlCombine.Combine((await this._Game.GetResourceAsync()).data.game.latest.decompressed_path.ToString(), "pkg_version"));
        HttpResponseMessage response = await Client.GetInstance().FetchAsync(pkgVersionRemoteUrl);

        if (response.IsSuccessStatusCode) {

            return PkgVersionParser.Parse(await response.Content.ReadAsStringAsync());

        } else {

            throw new GameException($"Failed to fetch the pkg_version file from remote servers (received HTTP status code {response.StatusCode})");

        }

    }

}