using Dodoco.Controller;
using Dodoco.Util;

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

        }

        public void Log(string message) {

            this.Write(LogType.LOG, Reflection.GetCallingMethod(), message);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = this.GetLastEntryJson(LogType.LOG)
            });

        }

        public void Log(string message, System.Exception? exception) {

            this.Write(LogType.LOG, Reflection.GetCallingMethod(), message, exception);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = this.GetLastEntryJson(LogType.LOG)
            });

        }

        public void Error(string message) {

            this.Write(LogType.ERROR, Reflection.GetCallingMethod(), message);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = this.GetLastEntryJson(LogType.ERROR)
            });

        }

        public void Error(string message, System.Exception? exception) {

            this.Write(LogType.ERROR, Reflection.GetCallingMethod(), message, exception);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = this.GetLastEntryJson(LogType.ERROR)
            });

        }

        public void Debug(string message) {

            this.Write(LogType.DEBUG, Reflection.GetCallingMethod(), message);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = this.GetLastEntryJson(LogType.DEBUG)
            });

        }

        public void Debug(string message, System.Exception? exception) {

            this.Write(LogType.DEBUG, Reflection.GetCallingMethod(), message, exception);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = this.GetLastEntryJson(LogType.DEBUG)
            });

        }

        public void Warning(string message) {

            this.Write(LogType.WARNING, Reflection.GetCallingMethod(), message);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = this.GetLastEntryJson(LogType.WARNING)
            });

        }

        public void Warning(string message, System.Exception? exception) {

            this.Write(LogType.WARNING, Reflection.GetCallingMethod(), message, exception);
            // Push event
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = GetLastEntryJson(LogType.WARNING)
            });

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

        public string GetLastEntryJson() {

            return Dodoco.Util.Text.StringUtil.StringToUTF8(JsonSerializer.Serialize<LogEntry>(logEntries.Last()));

        }

        public string? GetLastEntryJson(LogType type) {

            string? jsonContent = null;

            foreach (LogEntry entry in logEntries) {

                if (entry.type == type)
                    jsonContent = Dodoco.Util.Text.StringUtil.StringToUTF8(JsonSerializer.Serialize<LogEntry>(entry));

            }

            return jsonContent;

        }

    }

}