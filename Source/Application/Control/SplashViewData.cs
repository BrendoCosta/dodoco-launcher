using Dodoco.Core.Launcher;
using Dodoco.Core.Util.Log;

namespace Dodoco.Application.Control {

    public class SplashViewData {

        public LogEntry? LogEntry {
            
            get {

                LogEntry e = Logger.GetInstance().GetLastLogEntry();
                
                if (e.type != LogType.DEBUG) {

                    return e;

                } else {

                    return null;

                }

            }

        }

    }

}