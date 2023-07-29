using Dodoco.Core.Launcher;
using Dodoco.Core.Util.Log;

namespace Dodoco.Application.Control {

    public class SplashViewData {

        public LogEntry? LogEntry {
            
            get => Logger.GetInstance().GetLastLogEntry(new List<LogType> { LogType.LOG, LogType.ERROR, LogType.WARNING });

        }

    }

}