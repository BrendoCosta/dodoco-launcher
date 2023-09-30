using Dodoco.Core.Serialization.Json;
using Dodoco.Core.Util.Log;

using System.Text;

namespace Dodoco.Core {

    public class LogFile: WritableManagedFile<LogEntry> {

        public LogFile(): base(
            "log",
            Constants.LOG_DIRECTORY,
            Constants.LOG_FILENAME
        ) {}

        public void StartWritingToLog() { Logger.GetInstance().OnWrite += this.AppendToFile; }
        public void StopWritingToLog() { Logger.GetInstance().OnWrite -= this.AppendToFile; }
        
        public override LogEntry Read() {

            return new LogEntry(LogType.LOG, "", "");

        }
        
        public override void Write(LogEntry entry) {

            /*
             * NOTE: Avoid calling Logger-related methods here, since this method
             * will be called every time Logger class write something ( see AppendToFile() )
            */

            string fullFilePath = Path.Join(this.Directory, this.FileName);

            try {

                File.AppendAllText(fullFilePath, entry.GetAsText() + "\n", Encoding.UTF8);

            } catch (Exception e) {

                throw new CoreException($"Failed to write {InternalName} file to storage", e);

            }

        }

        private void AppendToFile(object? sender, LogEntry entry) {

            this.Write(entry);

        }

    }

}