using Dodoco.Core.Game;
using Dodoco.Core.Launcher;

namespace Dodoco.Application.Control {

    public class MainController: IController<MainViewData> {

        private ILauncher launcherInstance;

        public MainViewData ViewData { get; private set; } = MainViewData.GetInstance();
        public MainViewData GetViewData() => this.ViewData;

        public MainController(ILauncher launcher) {
            
            this.launcherInstance = launcher;
            this.ViewData.Launcher = launcher;
            this.ViewData.Game = launcher.GetGame();

        }

        public string GetBackroundImage() {

            return Convert.ToBase64String(((Launcher) this.launcherInstance).BackgroundImageFile.Read());

        }

    }

}