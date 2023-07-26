using Dodoco.Core.Game;
using Dodoco.Core.Launcher.Settings;
using Dodoco.Core.Wine;

namespace Dodoco.Core.Launcher {

    public interface ILauncher: IStatefulEntity<LauncherState> {

        void Stop(Exception? e = null);
        
        event EventHandler<LauncherDependency> OnDependenciesUpdated;

        LauncherSettings Settings { get; set; }
        LauncherDependency Dependency { get; }
        IGame? Game { get; }
        IWine? Wine { get; }

    }

}