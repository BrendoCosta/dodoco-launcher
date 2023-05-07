using Dococo.Util.Log;

namespace Dodoco.Launcher {

    public record LauncherSettings {

        public Launcher launcher = new Launcher();
        public Wine wine = new Wine();
        public Game game = new Game();
        public Api api = new Api();

        public record Launcher {

            public string language = "en-us";
            public bool display_splash_screen = true;

        }

        public record Wine {

            public string prefix_path = Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, "wine");

        }

        public record Game {

            public string server = "global";
            public string installation_path = Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, "game");

        }

        public record Api {

            public Dictionary<string, CompanyApi> company = new Dictionary<string, CompanyApi> {

                { "global", new CompanyApi() },
                { "chinese", new CompanyApi {

                    url = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9zZGstc3RhdGljLm1paG95by5jb20vaGs0ZV9jbi9tZGsvbGF1bmNoZXIvYXBp")),
                    key = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("ZVlkODlKbUo=")),
                    launcher_id = 18

                }},

            };

            public record CompanyApi {

                public string url = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9oazRlLWxhdW5jaGVyLXN0YXRpYy5ob3lvdmVyc2UuY29tL2hrNGVfZ2xvYmFsL21kay9sYXVuY2hlci9hcGk="));
                public string key = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("Z2NTdGdhcmg="));
                public int launcher_id = 10;

            }
            
        }

        public bool SearchSettingsFile() {

            Logger.GetInstance().Log($"Trying to find launcher's settings directory ({LauncherConstants.LAUNCHER_HOME_DIRECTORY})...");
            
            if (Directory.Exists(LauncherConstants.LAUNCHER_HOME_DIRECTORY)) {

                Logger.GetInstance().Log("Successfully found launcher's settings directory");
                Logger.GetInstance().Log($"Trying to find launcher's settings file ({Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, LauncherConstants.LAUNCHER_SETTINGS_FILENAME)})...");

                if (File.Exists(Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, LauncherConstants.LAUNCHER_SETTINGS_FILENAME))) {

                    Logger.GetInstance().Log("Successfully found launcher's settings file");

                } else {

                    Logger.GetInstance().Warning("Unable to find launcher's settings file. Will be assumed as first time initialization");
                    return false;

                }

            } else {

                Logger.GetInstance().Warning("Unable to find launcher's settings directory. Will be assumed as first time initialization");
                return false;

            }

            return true;

        }

        public LauncherSettings LoadSettings() {

            Logger.GetInstance().Log("Loading launcher's settings...");

            string fullFilePath = Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, LauncherConstants.LAUNCHER_SETTINGS_FILENAME);

            Logger.GetInstance().Log("Reading launcher's settings from settings file...");
            string settingsText = "";

            try {

                settingsText = File.ReadAllText(fullFilePath, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log("Succesfully read launcher's settings from settings file");
                

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to read launcher's settings from settings file", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            Logger.GetInstance().Log("Parsing launcher's settings...");
            LauncherSettings settings = new LauncherSettings();

            try {

                YamlDotNet.Serialization.IDeserializer des = new YamlDotNet.Serialization.DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                settings = des.Deserialize<LauncherSettings>(settingsText);
                Logger.GetInstance().Log("Successfully parsed launcher's settings");

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to parse launcher's settings", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            Logger.GetInstance().Log("Successfully loaded launcher's settings");

            return settings;

            //System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            //byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Hellow world"));
            //Logger.GetInstance().Debug(System.Convert.ToHexString(data));

        }

        public bool WriteDefaultSettings() {

            if (!Directory.Exists(LauncherConstants.LAUNCHER_HOME_DIRECTORY)) {

                Logger.GetInstance().Log($"Creating launcher's settings directory ({LauncherConstants.LAUNCHER_HOME_DIRECTORY})...");

                try {

                    Directory.CreateDirectory(LauncherConstants.LAUNCHER_HOME_DIRECTORY);
                    Logger.GetInstance().Log("Successfully created launcher's settings directory");

                } catch (Exception e) {

                    Logger.GetInstance().Error("Failed to create launcher's settings directory", e);
                    Dodoco.Application.Application.GetInstance().End(1);

                }

            }

            string fullFilePath = Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, LauncherConstants.LAUNCHER_SETTINGS_FILENAME);

            if (!File.Exists(fullFilePath)) {

                Logger.GetInstance().Log($"Creating launcher's settings file ({Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, LauncherConstants.LAUNCHER_SETTINGS_FILENAME)})...");

                try {

                    File.Create(fullFilePath).Close();
                    Logger.GetInstance().Log("Successfully created launcher's settings file");

                } catch (Exception e) {

                    Logger.GetInstance().Error("Failed to create launcher's settings file", e);
                    Dodoco.Application.Application.GetInstance().End(1);

                }

            }

            Logger.GetInstance().Log("Loading launcher's default settings...");

            string defaultSettingsYaml = "";

            try {

                YamlDotNet.Serialization.Serializer ser = new YamlDotNet.Serialization.Serializer();
                defaultSettingsYaml = ser.Serialize(new Dodoco.Launcher.LauncherSettings());
                Logger.GetInstance().Log("Successfully loaded launcher's default settings");

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to load launcher's default settings", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            Logger.GetInstance().Log("Writing launcher's default settings to settings file...");

            try {

                File.WriteAllText(fullFilePath, defaultSettingsYaml, System.Text.Encoding.UTF8);
                Logger.GetInstance().Log("Successfully wrote launcher's default settings to settings file");

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to write launcher's default settings into the settings file", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            return true;

        }

    }

}