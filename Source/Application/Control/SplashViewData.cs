using Dodoco.Core.Launcher;
using Dodoco.Core.Util.Log;

namespace Dodoco.Application.Control {

    public class SplashViewData {

        private ILauncher launcherInstance;

        public LauncherState _LauncherState { get => this.launcherInstance.State; }
        public LogEntry? LogEntry { get; set; } = null;

        public SplashViewData(ILauncher launcher) => this.launcherInstance = launcher;

    }

}