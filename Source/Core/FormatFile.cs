using Dodoco.Core.Serialization;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core {

    public abstract class FormatFile<T>: ManagedFile {

        protected IFormatSerializer Serializer;

        public FormatFile(string internalName, string directory, string fileName, IFormatSerializer serializer): base(internalName, directory, fileName) {
            
            this.Serializer = serializer;

        }

        public virtual T Read() {

            Logger.GetInstance().Log($"Loading {internalName} file...");

            string fullFilePath = Path.Join(this.directory, this.fileName);

            Logger.GetInstance().Log($"Reading {internalName} from {internalName} file...");
            string fileContents = "";

            try {

                fileContents = File.ReadAllText(fullFilePath, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Succesfully read {internalName} from {internalName} file");
                

            } catch (Exception e) {

                throw new CoreException($"Failed to read {internalName} from {internalName} file", e);

            }

            Logger.GetInstance().Log($"Parsing {internalName} file...");

            try {

                T content = Serializer.Deserialize<T>(fileContents);
                Logger.GetInstance().Log($"Successfully loaded {internalName} file");
                return content;

            } catch (Exception e) {

                throw new CoreException($"Failed to parse {internalName} file", e);

            }

        }

        public virtual void Write(T content) {

            Logger.GetInstance().Log($"Updating {internalName} file...");

            string fullFilePath = Path.Join(this.directory, this.fileName);
            string fileContents = "";

            try {

                if (content != null)
                    fileContents = Serializer.Serialize(content);
                    
                Logger.GetInstance().Log($"Successfully serialized {internalName} file");

            } catch (Exception e) {

                throw new CoreException($"Failed to serialize {internalName} file", e);

            }

            Logger.GetInstance().Log($"Writing {internalName} file to storage...");

            try {

                File.WriteAllText(fullFilePath, fileContents, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Successfully wrote {internalName} file to storage");

            } catch (Exception e) {

                throw new CoreException($"Failed to write {internalName} file to storage", e);

            }

            Logger.GetInstance().Log($"Successfully updated {internalName} file");

        }

    }

}