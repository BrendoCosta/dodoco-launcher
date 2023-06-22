using Dodoco.Core;
using Dodoco.Core.Game;
using Dodoco.Core.Launcher;
using Dodoco.Core.Network.HTTP;

namespace Dodoco.Application.Control {

    public class MainViewData {

        private static MainViewData? Instance = null;

        public ILauncher? Launcher { get; set; }
        public IGame? Game { get; set; }
        public ProgressReport ApplicationProgressReport { get; set; } = new ProgressReport();
        public DownloadProgressReport DownloadProgressReport { get; set; } = new DownloadProgressReport();

        private MainViewData() {}
        public static MainViewData GetInstance() {

            if (MainViewData.Instance == null) {

                MainViewData.Instance = new MainViewData();

            }

            return MainViewData.Instance;

        }

    }

}