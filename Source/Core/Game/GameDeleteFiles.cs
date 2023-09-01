namespace Dodoco.Core.Game {

    public class GameDeleteFiles: WritableManagedFile<List<string>> {

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

        public override void Write(List<string> content) {

            throw new NotSupportedException();

        }

    }

}