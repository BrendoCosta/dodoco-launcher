using Dodoco.Core.Serialization;
using Dodoco.Core.Serialization.Json;

namespace Dodoco.Core.Game {

    public class GameHdiffFiles: ReadableManagedFile<List<GameHdiffFilesEntry>> {

        public GameHdiffFiles(string gameInstallationDirectory): base(
            "hdifffiles",
            gameInstallationDirectory,
            "hdifffiles.txt"
        ) {}

        public override List<GameHdiffFilesEntry> Read() {

            List<GameHdiffFilesEntry> result = new List<GameHdiffFilesEntry>();
            IFormatSerializer serializer = new JsonSerializer();

            foreach (string line in File.ReadAllLines(this.FullPath)) {

                result.Add(serializer.Deserialize<GameHdiffFilesEntry>(line));

            }

            return result;

        }

    }

}