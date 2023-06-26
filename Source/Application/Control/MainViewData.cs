using Dodoco.Core;
using Dodoco.Core.Game;
using Dodoco.Core.Launcher;

namespace Dodoco.Application.Control {

    public class MainViewData {

        private static MainViewData? Instance = null;

        public ILauncher? Launcher { get; set; }
        public IGame? Game { get; set; }
        public ProgressReport ProgressReport { get; set; } = new ProgressReport();

        private MainViewData() {}
        public static MainViewData GetInstance() {

            if (MainViewData.Instance == null) {

                MainViewData.Instance = new MainViewData();

            }

            return MainViewData.Instance;

        }

    }

}