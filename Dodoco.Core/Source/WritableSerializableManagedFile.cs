using Dodoco.Core.Serialization;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core {

    public abstract class WritableSerializableManagedFile<T>: ReadableSerializableManagedFile<T> {

        public WritableSerializableManagedFile(string internalName, string directory, string fileName, IFormatSerializer serializer): base(internalName, directory, fileName, serializer) {}

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

        public virtual void Write(T content) {

            Logger.GetInstance().Log($"Updating {InternalName} file...");

            string fullFilePath = Path.Join(this.Directory, this.FileName);
            string fileContents = "";

            try {

                if (content != null)
                    fileContents = Serializer.Serialize(content);
                    
                Logger.GetInstance().Log($"Successfully serialized {InternalName} file");

            } catch (Exception e) {

                throw new CoreException($"Failed to serialize {InternalName} file", e);

            }

            Logger.GetInstance().Log($"Writing {InternalName} file to storage...");

            try {

                File.WriteAllText(fullFilePath, fileContents, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Successfully wrote {InternalName} file to storage");

            } catch (Exception e) {

                throw new CoreException($"Failed to write {InternalName} file to storage", e);

            }

            Logger.GetInstance().Log($"Successfully updated {InternalName} file");

        }

    }

}