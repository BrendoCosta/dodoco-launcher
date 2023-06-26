namespace Dodoco.Core.Game {

    public class GameDeleteFiles: ReadableManagedFile<List<string>> {

        public GameDeleteFiles(string gameInstallationDirectory): base(
            "deletefiles",
            gameInstallationDirectory,
            "deletefiles.txt"
        ) {}

        public override List<string> Read() {

            List<string> result = new List<string>();

            foreach (string line in File.ReadAllLines(this.FullPath)) {

                result.Add(line);

            }

            return result;

        }

    }

}