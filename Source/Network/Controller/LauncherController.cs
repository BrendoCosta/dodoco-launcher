using Dodoco.Application;
using Dodoco.Game;
using Dodoco.Launcher;
using Dodoco.Launcher.Cache;
using Dodoco.Launcher.Settings;
using Dodoco.Network.Api.Company.Launcher.Content;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Network.HTTP;

namespace Dodoco.Network.Controller {

    public sealed class LauncherController: ILauncher, IEntityInstanceController<ILauncher> {

        public ApplicationProgressReport LastGameCheckIntegrityProgressReport { get; private set; } = new ApplicationProgressReport();
        public DownloadProgressReport LastGameDownloadProgressReport { get; private set; } = new DownloadProgressReport();

        public event EventHandler<IGame> OnGameCreated = delegate {};

        public LauncherController() {

            this.GetEntityInstance().OnGameCreated += (object? sender, IGame game) => {

                game.OnCheckIntegrityProgress += (object? sender, ApplicationProgressReport e) => this.LastGameCheckIntegrityProgressReport = e;
                game.OnDownloadProgress += (object? sender, DownloadProgressReport e) => this.LastGameDownloadProgressReport = e;
                this.OnGameCreated.Invoke(sender, game);

            };

        }

        public ILauncher GetEntityInstance() => (ILauncher) Launcher.Launcher.GetInstance();
        public ApplicationProgressReport GetLastGameCheckIntegrityProgressReport() => this.LastGameCheckIntegrityProgressReport;
        public DownloadProgressReport GetLastGameDownloadProgressReport() => this.LastGameDownloadProgressReport;

        public void SetLauncherCache(LauncherCache cache) => this.GetEntityInstance().SetLauncherCache(cache);
        public void SetLauncherSettings(LauncherSettings settings) => this.GetEntityInstance().SetLauncherSettings(settings);

        public bool IsRunning() => this.GetEntityInstance().IsRunning();
        public LauncherActivityState GetLauncherActivityState() => this.GetEntityInstance().GetLauncherActivityState();
        public LauncherCache GetLauncherCache() => this.GetEntityInstance().GetLauncherCache();
        public Content GetContent() => this.GetEntityInstance().GetContent();
        public LauncherExecutionState GetLauncherExecutionState() => this.GetEntityInstance().GetLauncherExecutionState();
        public IGame GetGame() => this.GetEntityInstance().GetGame();
        public Resource GetResource() => this.GetEntityInstance().GetResource();
        public LauncherSettings GetLauncherSettings() => this.GetEntityInstance().GetLauncherSettings();
        public async Task RepairGameFiles() => this.GetEntityInstance().RepairGameFiles();

    }

}