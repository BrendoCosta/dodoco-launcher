using Dodoco.Core;
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

        public async Task UpdateGame() {

            ProgressReporter<ProgressReport> progress = new ProgressReporter<ProgressReport>();
            progress.ProgressChanged += (object? sender, ProgressReport e) => this.ViewData.ProgressReport = e;

            await this.launcherInstance.GetGame()?.Update(progress)!;
            
        }

    }

}