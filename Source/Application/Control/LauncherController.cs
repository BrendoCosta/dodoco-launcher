using Dodoco.Core.Launcher;
using Dodoco.Core.Launcher.Settings;

namespace Dodoco.Application.Control {

    public class LauncherController {

        private ILauncher launcher;

        public LauncherController(ILauncher launcher) => this.launcher = launcher;

        public LauncherSettings GetLauncherSettings() => this.launcher.Settings;
        public void SetLauncherSettings(LauncherSettings settings) => this.launcher.Settings = settings;
        public string GetLauncherBackgroundImage() => Convert.ToBase64String(((Launcher) this.launcher).BackgroundImageFile.Read());
        public LauncherDependency GetLauncherDependency() => this.launcher.Dependency;
        public LauncherState GetLauncherState() => this.launcher.State;

    }

}