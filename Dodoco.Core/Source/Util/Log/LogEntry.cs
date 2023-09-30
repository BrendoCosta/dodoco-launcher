namespace Dodoco.Core.Util.Log {

    public class LogEntry {

        public LogType type { get; private set; }
        public string prependMessage { get; private set; }
        public string message { get; private set; }

        public LogEntry(LogType type, string message, string method) {

            this.type = type;
            this.prependMessage = this.GetPrepend(type, method);
            this.message = message;

        }

        private string GetPrepend(LogType type, string method) {

            string timestamp = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            return $"{timestamp} [{type.ToString()} @ {method}] :";

        }

        public string GetAsText() {

            return $"{this.prependMessage} {this.message}";

        }

    }

}