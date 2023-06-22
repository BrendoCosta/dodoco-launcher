using Dodoco.Core.Serialization;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core {

    public abstract class ManagedFile {

        protected string internalName;
        protected string directory;
        protected string fileName;

        public ManagedFile(string internalName, string directory, string fileName) {

            this.internalName = internalName;
            this.directory = directory;
            this.fileName = fileName;

        }

        public bool Exists() {

            Logger.GetInstance().Log($"Trying to find {internalName} directory ({this.directory})...");
            
            if (Directory.Exists(this.directory)) {

                Logger.GetInstance().Log($"Successfully found {internalName} directory");
                Logger.GetInstance().Log($"Trying to find {internalName} file ({Path.Join(this.directory, this.fileName)})...");

                if (File.Exists(Path.Join(this.directory, this.fileName))) {

                    Logger.GetInstance().Log($"Successfully found {internalName} file");

                } else {

                    Logger.GetInstance().Warning($"Unable to find {internalName} file.");
                    return false;

                }

            } else {

                Logger.GetInstance().Warning($"Unable to find {internalName} directory.");
                return false;

            }

            return true;

        }

        public void Create() {

            if (!Directory.Exists(this.directory)) {

                Logger.GetInstance().Log($"Creating {internalName} directory ({this.directory})...");

                try {

                    Directory.CreateDirectory(this.directory);
                    Logger.GetInstance().Log($"Successfully created {internalName} directory");

                } catch (Exception e) {

                    throw new CoreException($"Failed to create {internalName} directory", e);

                }

            }

            string fullFilePath = Path.Join(this.directory, this.fileName);

            if (!File.Exists(fullFilePath)) {

                Logger.GetInstance().Log($"Creating {internalName} file ({Path.Join(this.directory, this.fileName)})...");

                try {

                    File.Create(fullFilePath).Close();
                    Logger.GetInstance().Log($"Successfully created {internalName} file");

                } catch (Exception e) {

                    throw new CoreException($"Failed to create {internalName} file", e);

                }

            } else {

                throw new CoreException($"The file {internalName} doesn't exists");

            }

        }

    }

}