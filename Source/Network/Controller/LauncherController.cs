using Dodoco.Game;
using Dodoco.Launcher;
using Dodoco.Launcher.Cache;
using Dodoco.Launcher.Settings;
using Dodoco.Network.Api.Company.Launcher.Content;
using Dodoco.Network.Api.Company.Launcher.Resource;

namespace Dodoco.Network.Controller {

    public sealed class LauncherController: IEntityInstanceController<ILauncher> {

        public ILauncher GetEntityInstance() {

            return (ILauncher) Launcher.Launcher.GetInstance();

        }

        public bool IsRunning() => this.GetEntityInstance().IsRunning();
        public LauncherActivityState GetLauncherActivityState() => this.GetEntityInstance().GetLauncherActivityState();
        public LauncherCache GetLauncherCache() => this.GetEntityInstance().GetLauncherCache();
        public Content GetContent() => this.GetEntityInstance().GetContent();
        public LauncherExecutionState GetLauncherExecutionState() => this.GetEntityInstance().GetLauncherExecutionState();
        public IGame GetGame() => this.GetEntityInstance().GetGame();
        public Resource GetResource() => this.GetEntityInstance().GetResource();
        public LauncherSettings GetLauncherSettings() => this.GetEntityInstance().GetLauncherSettings();
        public Task RepairGameFiles() => this.RepairGameFiles();

    }

}