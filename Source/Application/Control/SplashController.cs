using Dodoco.Core.Launcher;
using Dodoco.Core.Util.Log;

namespace Dodoco.Application.Control {

    public class SplashController: IController<SplashViewData> {

        public SplashViewData ViewData { get; private set; }
        public SplashViewData GetViewData() => this.ViewData;

        public SplashController(ILauncher launcher) {

            this.ViewData = new SplashViewData(launcher);

            Logger.GetInstance().OnWrite += (object? sender, LogEntry e) => {

                if (e.type != LogType.DEBUG)
                    this.ViewData.LogEntry = e;

            };

        }

    }

}