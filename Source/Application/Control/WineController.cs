using Dodoco.Core;
using Dodoco.Core.Launcher;
using Dodoco.Core.Network.Api.Github.Repos.Release;
using Dodoco.Core.Wine;
using Dodoco.Core.Util.Log;

namespace Dodoco.Application.Control {

    public class WineController: IController<WineControllerViewData> {

        private ILauncher launcher;
        public static WineControllerViewData ViewData { get; private set; } = new WineControllerViewData();
        private IWinePackageManager? packageManager { get => ((Launcher) this.launcher).WinePackageManager; }

        public WineController(ILauncher launcher) => this.launcher = launcher;

        public WineControllerViewData GetViewData() => ViewData;

        public WinePackageManagerState? GetWinePackageManagerState() => this.packageManager?.State;

        public async Task<List<Release>> GetAvaliableReleases() {

            if (this.packageManager == null)
                throw new UninitializedEntityException();

            return await this.packageManager.GetAvaliableReleases();

        }

        public async Task<Release> GetLatestRelease() {

            if (this.packageManager == null)
                throw new UninitializedEntityException();

            return await this.packageManager.GetLatestRelease();

        }

        public List<string> GetInstalledTags() {

            if (this.packageManager == null)
                throw new UninitializedEntityException();

             return this.packageManager.GetInstalledTags();

        }

        public async Task InstallLatestRelease() {

            if (this.packageManager == null)
                throw new UninitializedEntityException();

            ProgressReporter<ProgressReport> progress = new ProgressReporter<ProgressReport>();
            progress.ProgressChanged += (object? sender, ProgressReport e) => MainController.ViewData._ProgressReport = e;

            try {

                await this.packageManager.InstallPackageFromRelease(await this.GetLatestRelease(), progress);

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to install package", e);
                throw;

            } finally {

                MainController.ViewData._ProgressReport = null;

            }

        }

        public async Task InstallPackageFromRelease(Release release) {

            if (this.packageManager == null)
                throw new UninitializedEntityException();

            ViewData.ReleaseDownloadProgressReport.Add(release.tag_name, new ProgressReport());

            ProgressReporter<ProgressReport> progress = new ProgressReporter<ProgressReport>();
            progress.ProgressChanged += (object? sender, ProgressReport e) => ViewData.ReleaseDownloadProgressReport[release.tag_name] = e;

            try {

                await this.packageManager.InstallPackageFromRelease(release, progress);

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to install package", e);
                throw;

            } finally {

                ViewData.ReleaseDownloadProgressReport.Remove(release.tag_name);

            }

        }

    }

}