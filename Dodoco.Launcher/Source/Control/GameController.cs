using Dodoco.Core;
using Dodoco.Core.Launcher;
using Dodoco.Core.Game;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Util.Log;

namespace Dodoco.Application.Control {

    public class GameController {

        private ILauncher launcher;

        public GameController(ILauncher launcher) => this.launcher = launcher;

        public GameState? GetGameState() => this.launcher.Game?.State;
        public GameDownloadState? GetGameDownloadState() => this.launcher.Game?.DownloadState;
        public GameUpdateState? GetGameUpdateState() => this.launcher.Game?.UpdateState;
        public GameIntegrityCheckState? GetGameIntegrityCheckState() => this.launcher.Game?.IntegrityCheckState;

        public List<GameServer> GetAvaliableGameServers() {

            return Enum.GetValues<GameServer>().ToList();

        }

        public async Task Download() {

            if (this.launcher.Game == null)
                throw new UninitializedEntityException();

            ProgressReporter<ProgressReport> progress = new ProgressReporter<ProgressReport>();
            // Reports the progress to the main view
            progress.ProgressChanged += (object? sender, ProgressReport report) => MainController.ViewData._ProgressReport = report;

            try {

                await this.launcher.Game.Download(progress);

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to download the game", e);
                throw;

            } finally {

                MainController.ViewData._ProgressReport = null;

            }
            
            return;

        }

        public async Task<ResourceGame?> GetUpdateAsync() {

            if (this.launcher.Game == null)
                throw new UninitializedEntityException();

            return await this.launcher.Game.GetUpdateAsync();

        }

        public async Task<ResourceGame?> GetPreUpdateAsync() {

            if (this.launcher.Game == null)
                throw new UninitializedEntityException();

            return await this.launcher.Game.GetPreUpdateAsync();

        }

        public async Task<bool> IsPreUpdateDownloadedAsync() {

            if (this.launcher.Game == null)
                throw new UninitializedEntityException();

            return await this.launcher.Game.IsPreUpdateDownloadedAsync();

        }

        public async Task UpdateAsync(bool isPreUpdate) {

            if (this.launcher.Game == null)
                throw new UninitializedEntityException();

            ProgressReporter<ProgressReport> reporter = new ProgressReporter<ProgressReport>();
            // Reports the progress to the main view
            reporter.ProgressChanged += (object? sender, ProgressReport report) => MainController.ViewData._ProgressReport = report;

            try {

                await this.launcher.Game.UpdateAsync(isPreUpdate, reporter, CancellationToken.None);

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to update the game", e);
                throw;

            } finally {

                MainController.ViewData._ProgressReport = null;

            }
            
            return;
            
        }

        public async Task RepairGameFiles() {

            if (this.launcher.Game == null)
                throw new UninitializedEntityException();

            ProgressReporter<ProgressReport> progress = new ProgressReporter<ProgressReport>();
            // Report the progress to the main view
            progress.ProgressChanged += (object? sender, ProgressReport report) => MainController.ViewData._ProgressReport = report;

            try {

                await this.launcher.Game.RepairGameFiles(progress);

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to repair the game", e);
                throw;

            } finally {

                MainController.ViewData._ProgressReport = null;

            }

            return;

        }

        public async Task Start() {

            if (this.launcher.Game == null)
                throw new UninitializedEntityException();

            try {

                await this.launcher.Game.Start();

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to start the game", e);
                throw;

            }

            return;

        }

    }

}