using Dodoco.Core;
using Dodoco.Core.Game;
using Dodoco.Core.Launcher;
using Dodoco.Core.Wine;

namespace Dodoco.Application.Control {

    public class MainViewData {

        //public GameState? _GameState { get => this.launcherInstance.Game?.State; }
        //public LauncherDependency _LauncherDependency { get => this.launcherInstance.Dependency; }
        //public LauncherState _LauncherState { get => this.launcherInstance.State; }
        //public WinePackageManagerState? _WinePackageManagerState { get => ((Launcher) this.launcherInstance).WinePackageManager?.State; }
        public ProgressReport? _ProgressReport { get; set; }

    }

}