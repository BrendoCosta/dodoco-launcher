using Dodoco.Core.Serialization;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core {

    public abstract class ManagedFile {

        public string InternalName { get; protected set; }
        public string Directory { get; protected set; }
        public string FileName { get; protected set; }
        public string FullPath { get; private set; }

        public ManagedFile(string internalName, string directory, string fileName) {

            this.InternalName = internalName;
            this.Directory = directory;
            this.FileName = fileName;
            this.FullPath = Path.Join(this.Directory, this.FileName);

        }

        public bool Exist() {

            Logger.GetInstance().Log($"Trying to find {InternalName} directory ({this.Directory})...");
            
            if (System.IO.Directory.Exists(this.Directory)) {

                Logger.GetInstance().Log($"Successfully found {InternalName} directory");
                Logger.GetInstance().Log($"Trying to find {InternalName} file ({Path.Join(this.Directory, this.FileName)})...");

                if (File.Exists(Path.Join(this.Directory, this.FileName))) {

                    Logger.GetInstance().Log($"Successfully found {InternalName} file");

                } else {

                    Logger.GetInstance().Warning($"Unable to find {InternalName} file.");
                    return false;

                }

            } else {

                Logger.GetInstance().Warning($"Unable to find {InternalName} directory.");
                return false;

            }

            return true;

        }

    }

}