using Dodoco.Util.Log;
using Dodoco.Util;

namespace Dodoco.Launcher {

    public abstract record LauncherFile {

        string internalName;
        string directory;
        string fileName;

        public LauncherFile(string internalName, string directory, string fileName) {

            this.internalName = internalName;
            this.directory = directory;
            this.fileName = fileName;

        }

        public bool Exists() {

            Logger.GetInstance().Log($"Trying to find launcher's {internalName} directory ({this.directory})...");
            
            if (Directory.Exists(this.directory)) {

                Logger.GetInstance().Log($"Successfully found launcher's {internalName} directory");
                Logger.GetInstance().Log($"Trying to find launcher's {internalName} file ({Path.Join(this.directory, this.fileName)})...");

                if (File.Exists(Path.Join(this.directory, this.fileName))) {

                    Logger.GetInstance().Log($"Successfully found launcher's {internalName} file");

                } else {

                    Logger.GetInstance().Warning($"Unable to find launcher's {internalName} file.");
                    return false;

                }

            } else {

                Logger.GetInstance().Warning($"Unable to find launcher's {internalName} directory.");
                return false;

            }

            return true;

        }

        public LauncherFile? Load<LauncherFile>() where LauncherFile: new() {

            Logger.GetInstance().Log($"Loading launcher's {internalName} file...");

            string fullFilePath = Path.Join(this.directory, this.fileName);

            Logger.GetInstance().Log($"Reading launcher's {internalName} from {internalName} file...");
            string fileContents = "";

            try {

                fileContents = File.ReadAllText(fullFilePath, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Succesfully read launcher's {internalName} from {internalName} file");
                

            } catch (Exception e) {

                Logger.GetInstance().Error($"Failed to read launcher's {internalName} from {internalName} file", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            Logger.GetInstance().Log($"Parsing launcher's {internalName} file...");
            LauncherFile? file = new LauncherFile();

            try {

                YamlDotNet.Serialization.IDeserializer des = new YamlDotNet.Serialization.DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .WithTypeConverter(new CultureInfoYamlConverter())
                    .Build();
                file = des.Deserialize<LauncherFile>(fileContents);
                Logger.GetInstance().Log($"Successfully parsed launcher's {internalName} file");

            } catch (Exception e) {

                Logger.GetInstance().Error($"Failed to parse launcher's {internalName} file", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            Logger.GetInstance().Log($"Successfully loaded launcher's {internalName} file");

            return file;

            //System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            //byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"Hellow world"));
            //Logger.GetInstance().Debug(System.Convert.ToHexString(data));

        }

        public bool WriteDefault<LauncherFile>() where LauncherFile: new() {

            if (!Directory.Exists(this.directory)) {

                Logger.GetInstance().Log($"Creating launcher's {internalName} directory ({this.directory})...");

                try {

                    Directory.CreateDirectory(this.directory);
                    Logger.GetInstance().Log($"Successfully created launcher's {internalName} directory");

                } catch (Exception e) {

                    Logger.GetInstance().Error($"Failed to create launcher's {internalName} directory", e);
                    Dodoco.Application.Application.GetInstance().End(1);

                }

            }

            string fullFilePath = Path.Join(this.directory, this.fileName);

            if (!File.Exists(fullFilePath)) {

                Logger.GetInstance().Log($"Creating launcher's {internalName} file ({Path.Join(this.directory, this.fileName)})...");

                try {

                    File.Create(fullFilePath).Close();
                    Logger.GetInstance().Log($"Successfully created launcher's {internalName} file");

                } catch (Exception e) {

                    Logger.GetInstance().Error($"Failed to create launcher's {internalName} file", e);
                    Dodoco.Application.Application.GetInstance().End(1);

                }

            }

            Logger.GetInstance().Log($"Loading launcher's default {internalName}...");

            string fileContents = "";

            try {

                YamlDotNet.Serialization.ISerializer ser = new YamlDotNet.Serialization.SerializerBuilder()
                    .WithTypeConverter(new CultureInfoYamlConverter())
                    .Build();
                fileContents = ser.Serialize(new LauncherFile());
                Logger.GetInstance().Log($"Successfully loaded launcher's default {internalName}");

            } catch (Exception e) {

                Logger.GetInstance().Error($"Failed to load launcher's default {internalName}", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            Logger.GetInstance().Log($"Writing launcher's default {internalName} to {internalName} file...");

            try {

                File.WriteAllText(fullFilePath, fileContents, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log($"Successfully wrote launcher's default {internalName} to {internalName} file");

            } catch (Exception e) {

                Logger.GetInstance().Error($"Failed to write launcher's default {internalName} into the {internalName} file", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            return true;

        }

    }

}