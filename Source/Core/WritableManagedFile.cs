using Dodoco.Core.Util.Log;

namespace Dodoco.Core {

    public abstract class WritableManagedFile<T>: ReadableManagedFile<T> {

        public WritableManagedFile(string internalName, string directory, string fileName): base(
            internalName,
            directory,
            fileName
        ) {}

        public void Create() {

            if (!System.IO.Directory.Exists(this.Directory)) {

                Logger.GetInstance().Log($"Creating {InternalName} directory ({this.Directory})...");

                try {

                    System.IO.Directory.CreateDirectory(this.Directory);
                    Logger.GetInstance().Log($"Successfully created {InternalName} directory");

                } catch (Exception e) {

                    throw new CoreException($"Failed to create {InternalName} directory", e);

                }

            }

            string fullFilePath = Path.Join(this.Directory, this.FileName);

            if (!File.Exists(fullFilePath)) {

                Logger.GetInstance().Log($"Creating {InternalName} file ({Path.Join(this.Directory, this.FileName)})...");

                try {

                    File.Create(fullFilePath).Close();
                    Logger.GetInstance().Log($"Successfully created {InternalName} file");

                } catch (Exception e) {

                    throw new CoreException($"Failed to create {InternalName} file", e);

                }

            } else {

                throw new CoreException($"The file {InternalName} doesn't exists");

            }

        }

        public void Delete() {

            if (File.Exists(this.FullPath)) {

                File.Delete(this.FullPath);

            } else {

                throw new CoreException($"Can't find the file \"{this.FullPath}\"");

            }

        }

        public abstract void Write(T content);

    }

}