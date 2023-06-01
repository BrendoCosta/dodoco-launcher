using Dodoco.Serialization.Json;
using Dodoco.Util.Log;

using System.Text;

namespace Dodoco.Application {

    public record ApplicationLog: ApplicationFile<object> {

        private Queue<LogEntry> linesQueue = new Queue<LogEntry>();

        public ApplicationLog(): base(
            "log",
            ApplicationConstants.APPLICATION_LOG_DIRECTORY,
            ApplicationConstants.APPLICATION_LOG_FILENAME,
            new JsonSerializer()
        ) {

            /*
             * This enqueue all entries created
             * since application start.
            

            foreach (LogEntry entry in Logger.Entries) {

                this.linesQueue.Enqueue(entry);

            }

            */

        }

        public override void LoadFile() {}
        
        public override void WriteFile() {

            /*
             * NOTE: Avoid calling Logger-related methods here, since this method
             * will be called every time Logger class write something ( see AppendToFile() )
            */

            string fullFilePath = Path.Join(this.directory, this.fileName);

            try {

                if (this.linesQueue.Count > 0) {

                    File.AppendAllText(fullFilePath, this.linesQueue.Dequeue().GetAsText() + "\n", Encoding.UTF8);

                }

            } catch (Exception e) {

                throw new ApplicationException($"Failed to write {internalName} file to storage", e);

            }

        }

        public void StartWritingToLog() { Logger.GetInstance().OnWrite += this.AppendToFile; }
        public void StopWritingToLog() { Logger.GetInstance().OnWrite -= this.AppendToFile; }

        private void AppendToFile(object? sender, LogEntry entry) {

            this.linesQueue.Enqueue(entry);
            this.WriteFile();

        }

    }

}