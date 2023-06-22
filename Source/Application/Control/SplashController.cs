using Dodoco.Core.Util.Log;

namespace Dodoco.Application.Control {

    public class SplashController: IController<SplashViewData> {

        public SplashViewData ViewData { get; private set; } = new SplashViewData();
        public SplashViewData GetViewData() => this.ViewData;

        public SplashController() {

            Logger.GetInstance().OnWrite += (object? sender, LogEntry e) => {

                if (e.type != LogType.DEBUG)
                    this.ViewData.LogEntry = e;

            };

        }

    }

}