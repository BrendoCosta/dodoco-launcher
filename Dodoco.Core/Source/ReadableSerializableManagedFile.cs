using Dodoco.Core.Serialization;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core {

    public abstract class ReadableSerializableManagedFile<T>: SerializableManagedFile {

        public ReadableSerializableManagedFile(string internalName, string directory, string fileName, IFormatSerializer serializer): base(internalName, directory, fileName, serializer) {}
        
        public virtual T Read() {

            Logger.GetInstance().Log($"Loading {InternalName} file...");

            string fullFilePath = Path.Join(this.Directory, this.FileName);

            Logger.GetInstance().Log($"Reading {InternalName} from {InternalName} file...");
            string fileContents = "";

            try {

                fileContents = File.ReadAllText(fullFilePath, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Successfully read {InternalName} from {InternalName} file");
                

            } catch (Exception e) {

                throw new CoreException($"Failed to read {InternalName} from {InternalName} file", e);

            }

            Logger.GetInstance().Log($"Parsing {InternalName} file...");

            try {

                T content = Serializer.Deserialize<T>(fileContents);
                Logger.GetInstance().Log($"Successfully loaded {InternalName} file");
                return content;

            } catch (Exception e) {

                throw new CoreException($"Failed to parse {InternalName} file", e);

            }

        }

    }

}