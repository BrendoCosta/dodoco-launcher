using Dodoco.Core;
using Dodoco.Core.Game;
using Dodoco.Core.Launcher;

namespace Dodoco.Application.Control {

    public class MainController: IController<MainViewData> {

        public static MainViewData ViewData { get; private set; } = new MainViewData();
        public MainViewData GetViewData() => ViewData;

    }

}