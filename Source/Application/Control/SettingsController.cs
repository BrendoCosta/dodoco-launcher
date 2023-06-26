using Dodoco.Core;
using Dodoco.Core.Game;
using Dodoco.Core.Launcher;
using Dodoco.Core.Launcher.Settings;
using Dodoco.Core.Network.Api.Github.Repos.Release;
using Dodoco.Core.Util.Log;
using Dodoco.Core.Wine;

namespace Dodoco.Application.Control {

    public class SettingsController: IController<SettingsViewData> {

        private ILauncher launcher;
        private IGame? game = null;
        private SettingsViewData ViewData = new SettingsViewData();

        public SettingsViewData GetViewData() => this.ViewData;

        public SettingsController(ILauncher launcher) {

            this.launcher = launcher;
            this.game = launcher.GetGame();

        }

        public LauncherSettings GetLauncherSettings() => this.launcher.GetLauncherSettings();
        public async Task SetLauncherSettings(LauncherSettings settings) {

            this.launcher.UpdateLauncherSettings(settings);
            await ((Launcher) this.launcher).ManageAllResources();

        }

        public async Task<List<GameFileIntegrityReport>?> CheckFilesIntegrity() {

            if (game == null) {

                Logger.GetInstance().Log("Game's instance not initialized");
                return null;

            }

            // Reports the progress to the main view

            ProgressReporter<ProgressReport> progress = new ProgressReporter<ProgressReport>();
            progress.ProgressChanged += (object? sender, ProgressReport report) => MainViewData.GetInstance().ProgressReport = report;
            
            return await game.CheckFilesIntegrity(progress);

        }

        public async Task<List<Release>?> GetAvaliableWineReleases() {

            IWinePackageManager? packageManager = ((Launcher) this.launcher).WinePackageManager;

            if (packageManager != null) {

                return await packageManager.GetAvaliableReleases();

            }

            return null;

        }

        public List<string>? GetInstalledWineTags() {

            IWinePackageManager? packageManager = ((Launcher) this.launcher).WinePackageManager;

            if (packageManager != null) {

                return packageManager.GetInstalledTags();

            }

            return null;

        }

        public async Task DownloadWine(Release release) {

            IWinePackageManager? packageManager = ((Launcher) this.launcher).WinePackageManager;

            if (packageManager != null) {

                this.ViewData.WineDownloadStatus.Add(release.tag_name, new ProgressReport());

                ProgressReporter<ProgressReport> progress = new ProgressReporter<ProgressReport>();
                progress.ProgressChanged += (object? sender, ProgressReport e) => this.ViewData.WineDownloadStatus[release.tag_name] = e;

                await packageManager.InstallPackageFromRelease(release, progress);

                this.ViewData.WineDownloadStatus.Remove(release.tag_name);

            }

        }

    }

}