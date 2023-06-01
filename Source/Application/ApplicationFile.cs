using Dodoco.Serialization;
using Dodoco.Util.Log;

namespace Dodoco.Application {

    public abstract record ApplicationFile<T> {

        protected string internalName;
        protected string directory;
        protected string fileName;
        protected IFormatSerializer serializer;
        public T Content { get; set; }

        public ApplicationFile(string internalName, string directory, string fileName, IFormatSerializer serializer) {

            this.internalName = internalName;
            this.directory = directory;
            this.fileName = fileName;
            this.serializer = serializer;
            this.Content = Activator.CreateInstance<T>();

        }

        public virtual bool Exists() {

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

        public virtual void LoadFile() {

            Logger.GetInstance().Log($"Loading {internalName} file...");

            string fullFilePath = Path.Join(this.directory, this.fileName);

            Logger.GetInstance().Log($"Reading {internalName} from {internalName} file...");
            string fileContents = "";

            try {

                fileContents = File.ReadAllText(fullFilePath, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Succesfully read {internalName} from {internalName} file");
                

            } catch (Exception e) {

                throw new ApplicationException($"Failed to read {internalName} from {internalName} file", e);

            }

            Logger.GetInstance().Log($"Parsing {internalName} file...");

            try {

                this.Content = serializer.Deserialize<T>(fileContents);
                Logger.GetInstance().Log($"Successfully parsed {internalName} file");

            } catch (Exception e) {

                throw new ApplicationException($"Failed to parse {internalName} file", e);

            }

            Logger.GetInstance().Log($"Successfully loaded {internalName} file");

        }

        public virtual void CreateFile() {

            if (!Directory.Exists(this.directory)) {

                Logger.GetInstance().Log($"Creating {internalName} directory ({this.directory})...");

                try {

                    Directory.CreateDirectory(this.directory);
                    Logger.GetInstance().Log($"Successfully created {internalName} directory");

                } catch (Exception e) {

                    throw new ApplicationException($"Failed to create {internalName} directory", e);

                }

            }

            string fullFilePath = Path.Join(this.directory, this.fileName);

            if (!File.Exists(fullFilePath)) {

                Logger.GetInstance().Log($"Creating {internalName} file ({Path.Join(this.directory, this.fileName)})...");

                try {

                    File.Create(fullFilePath).Close();
                    Logger.GetInstance().Log($"Successfully created {internalName} file");

                } catch (Exception e) {

                    throw new ApplicationException($"Failed to create {internalName} file", e);

                }

            } else {

                throw new ApplicationException($"The file {internalName} doesn't exists");

            }

        }

        public virtual void WriteFile() {

            Logger.GetInstance().Log($"Updating {internalName} file...");

            string fullFilePath = Path.Join(this.directory, this.fileName);
            string fileContents = "";

            try {

                if (this.Content != null)
                    fileContents = serializer.Serialize(this.Content);
                    
                Logger.GetInstance().Log($"Successfully serialized {internalName} file");

            } catch (Exception e) {

                throw new ApplicationException($"Failed to serialize {internalName} file", e);

            }

            Logger.GetInstance().Log($"Writing {internalName} file to storage...");

            try {

                File.WriteAllText(fullFilePath, fileContents, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Successfully wrote {internalName} file to storage");

            } catch (Exception e) {

                throw new ApplicationException($"Failed to write {internalName} file to storage", e);

            }

            Logger.GetInstance().Log($"Successfully updated {internalName} file");

        }

    }

}