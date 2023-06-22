namespace Dodoco.Core.Util.Log {

    public sealed class Logger {

        public static List<LogEntry> Entries = new List<LogEntry>();
        public event EventHandler<LogEntry> OnWrite = delegate {};
        public event EventHandler<LogEntry> OnLog = delegate {};
        public event EventHandler<LogEntry> OnDebug = delegate {};
        public event EventHandler<LogEntry> OnError = delegate {};
        public event EventHandler<LogEntry> OnWarning = delegate {};
        private static Logger? instance = null;

        public static Logger GetInstance() {

            if (instance == null) {

                instance = new Logger();
            }

            return instance;
            
        }

        private Logger() {}

        public void Log(string message) {

            this.Write(LogType.LOG, Reflection.GetCallingMethod(), message);

        }

        public void Log(string message, System.Exception? exception) {

            this.Write(LogType.LOG, Reflection.GetCallingMethod(), message, exception);

        }

        public void Error(string message) {

            this.Write(LogType.ERROR, Reflection.GetCallingMethod(), message);

        }

        public void Error(string message, System.Exception? exception) {

            this.Write(LogType.ERROR, Reflection.GetCallingMethod(), message, exception);

        }

        public void Debug(string message) {

            this.Write(LogType.DEBUG, Reflection.GetCallingMethod(), message);

        }

        public void Debug(string message, System.Exception? exception) {

            this.Write(LogType.DEBUG, Reflection.GetCallingMethod(), message, exception);

        }

        public void Warning(string message) {

            this.Write(LogType.WARNING, Reflection.GetCallingMethod(), message);

        }

        public void Warning(string message, System.Exception? exception) {

            this.Write(LogType.WARNING, Reflection.GetCallingMethod(), message, exception);

        }

        private void Write(LogType type, string method, string message) {

            Write(type, method, message, null);

        }

        private void Write(LogType type, string method, string message, System.Exception? exception) {

            // Add the new log entry
            
            LogEntry newEntry = new LogEntry(type, message, method);
            Entries.Add(newEntry);

            // Pass event to listners with the new entry as the argument

            switch (type) {

                case LogType.LOG:
                    this.OnLog.Invoke(this, newEntry);
                    break;
                case LogType.ERROR:
                    this.OnError.Invoke(this, newEntry);
                    break;
                case LogType.WARNING:
                    this.OnWarning.Invoke(this, newEntry);
                    break;
                case LogType.DEBUG:
                    this.OnDebug.Invoke(this, newEntry);
                    break;

            }

            this.OnWrite.Invoke(this, newEntry);

            // Write the entry to the STDOUT
            // TODO: turn this a optional feature through LauncherSettings
            
            ConsoleColor color = ConsoleColor.Gray;

            switch (type) {

                case LogType.LOG:
                    color = ConsoleColor.Gray;
                    break;
                case LogType.ERROR:
                    color = ConsoleColor.Red;
                    break;
                case LogType.WARNING:
                    color = ConsoleColor.Yellow;
                    break;
                case LogType.DEBUG:
                    color = ConsoleColor.Cyan;
                    break;

            }
            
            Console.ForegroundColor = color;
            Console.WriteLine(newEntry.GetAsText());
            Console.ForegroundColor = ConsoleColor.Gray;

            // Log exceptions and inner exceptions

            if (exception != null) {

                Exception? e = exception;
                
                this.Debug($"=========== DEBUG START ===========");

                while (e != null) {

                    this.Debug($"EXCEPTION MESSAGE: {e.Message}");
                    this.Debug($"EXCEPTION TYPE: {e.GetType().ToString()}");
                    this.Debug($"EXCEPTION STRACK TRACE: {e.StackTrace}");
                    e = e.InnerException ?? null;

                }

                this.Debug($"===========  DEBUG END  ===========");

            }

        }

        public string GetFullLogText() {

            List<String> fullLog = new List<String>();

            foreach (LogEntry entry in Entries) {

                fullLog.Add($"{entry.prependMessage} {entry.message}");

            }

            return String.Join("\n", fullLog.ToArray());

        }

        public LogEntry GetLastLogEntry() {

            return Entries.Last();

        }

        public LogEntry? GetLastLogEntry(LogType type) {

            foreach (LogEntry entry in Entries) {

                if (entry.type == type)
                    return entry;

            }

            return null;

        }

    }

}