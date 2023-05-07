using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Util.Log {

    public sealed class Logger {

        private static List<LogEntry> logEntries = new List<LogEntry>();
        private static Logger? instance = null;

        public static Logger GetInstance() {

            if (instance == null) {

                instance = new Logger();
            }

            return instance;
            
        }

        private Logger() {

            Dodoco.Controller.ServerSentEvents.RegisterEvent("Dodoco.Util.Log.Logger.GetLastLogJson", () => {

                return new Dodoco.HTTP.SSE.Event {

                    eventName = "Dodoco.Util.Log.Logger.GetLastLogJson",
                    data = GetLastLogJson()
                    
                };

            });

        }

        public void Log(string message) {

            this.Write(LogType.LOG, this.GetCallingMethod(), message);

        }

        public void Log(string message, System.Exception? exception) {

            this.Write(LogType.LOG, this.GetCallingMethod(), message, exception);

        }

        public void Error(string message) {

            this.Write(LogType.ERROR, this.GetCallingMethod(), message);

        }

        public void Error(string message, System.Exception? exception) {

            this.Write(LogType.ERROR, this.GetCallingMethod(), message, exception);

        }

        public void Debug(string message) {

            this.Write(LogType.DEBUG, this.GetCallingMethod(), message);

        }

        public void Debug(string message, System.Exception? exception) {

            this.Write(LogType.DEBUG, this.GetCallingMethod(), message, exception);

        }

        public void Warning(string message) {

            this.Write(LogType.WARNING, this.GetCallingMethod(), message);

        }

        public void Warning(string message, System.Exception? exception) {

            this.Write(LogType.WARNING, this.GetCallingMethod(), message, exception);

        }

        private void Write(LogType type, string method, string message) {

            Write(type, method, message, null);

        }

        private void Write(LogType type, string method, string message, System.Exception? exception) {

            logEntries.Add(new LogEntry(type, message, method));
            
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
            Console.WriteLine($"{logEntries.Last().prependMessage} {logEntries.Last().message}");
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

        private string GetCallingMethod() {

            MethodBase? methodInfo = new System.Diagnostics.StackTrace().GetFrame(2)?.GetMethod();

            string className = "UnknownClass";
            string methodName = "UnknownMethod";

            if (methodInfo != null) {

                methodName = methodInfo.IsConstructor ? "Constructor" : methodInfo.Name;
                
                if (methodInfo.ReflectedType?.FullName != null) {

                    className = methodInfo.ReflectedType.FullName;

                } else if (methodInfo.ReflectedType != null) {

                    className = methodInfo.ReflectedType.Name;

                }

            }

            return $"{className}.{methodName}";

        }

        public string GetFullLogText() {

            List<String> fullLog = new List<String>();

            foreach (LogEntry entry in logEntries) {

                fullLog.Add($"{entry.prependMessage} {entry.message}");

            }

            return Dodoco.Util.Text.StringUtil.StringToUTF8(String.Join("\n", fullLog.ToArray()));

        }

        public string GetFullLogJson() {

            return Dodoco.Util.Text.StringUtil.StringToUTF8(JsonSerializer.Serialize<List<LogEntry>>(logEntries));

        }

        public string GetLastLogJson() {

            return Dodoco.Util.Text.StringUtil.StringToUTF8(JsonSerializer.Serialize<LogEntry>(logEntries.Last()));

        }


    }

}