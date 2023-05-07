using Dococo.Util.Log;
using System.Text.Json;

namespace Dodoco.Launcher {

    public class Launcher {

        private static Launcher? instance = null;

        // https://github.com/Raxdiam/photino.API

        private PhotinoNET.PhotinoWindow? window = null;
        private Thread? windowThread;
        private bool running = false;
        public static LauncherExecutionState executionState { get; private set; } = LauncherExecutionState.UNINITIALIZED;
        public static LauncherActivityState activityState { get; private set; } = LauncherActivityState.UNREADY;
        public static LauncherSettings settings = new LauncherSettings();

        public static Launcher GetInstance() {

            if (instance == null) {

                instance = new Launcher();

            }

            return instance;
            
        }

        private Launcher() {

            Logger.GetInstance().Log("Initializing launcher...");

            Logger.GetInstance().Log("Registering launcher's events...");
            
            Dodoco.Controller.ServerSentEvents.RegisterEvent("Dodoco.Launcher.Launcher.executionState", () => {

                return new Dodoco.HTTP.SSE.Event {

                    eventName = $"Dodoco.Launcher.Launcher.executionState",
                    data = executionState.ToString()
                    
                };

            });

            Dodoco.Controller.ServerSentEvents.RegisterEvent("Dodoco.Launcher.Launcher.activityState", () => {

                return new Dodoco.HTTP.SSE.Event {

                    eventName = $"Dodoco.Launcher.Launcher.activityState",
                    data = activityState.ToString()
                    
                };

            });

            this.UpdateExecutionState(LauncherExecutionState.INITIALIZING);
            this.CreateThread();

        }

        private void SearchSettings() {

            Logger.GetInstance().Log($"Trying to find launcher's settings directory ({LauncherConstants.LAUNCHER_HOME_DIRECTORY})...");
            
            if (Directory.Exists(LauncherConstants.LAUNCHER_HOME_DIRECTORY)) {

                Logger.GetInstance().Log("Successfully found launcher's settings directory");
                Logger.GetInstance().Log($"Trying to find launcher's settings file ({Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, LauncherConstants.LAUNCHER_SETTINGS_FILENAME)})...");

                if (File.Exists(Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, LauncherConstants.LAUNCHER_SETTINGS_FILENAME))) {

                    Logger.GetInstance().Log("Successfully found launcher's settings file");

                } else {

                    Logger.GetInstance().Warning("Unable to find launcher's settings file. Will be assumed as first time initialization");
                    this.WriteDefaultSettings();

                }

            } else {

                Logger.GetInstance().Warning("Unable to find launcher's settings directory. Will be assumed as first time initialization");
                this.WriteDefaultSettings();

            }

            this.ReadSettings();

        }

        private void ReadSettings() {

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

            Logger.GetInstance().Log("Parsing and loading launcher's settings...");
            bool parsingSuccess = false;

            try {

                YamlDotNet.Serialization.IDeserializer des = new YamlDotNet.Serialization.DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                settings = des.Deserialize<LauncherSettings>(settingsText);
                Logger.GetInstance().Log("Successfully parsed and loaded launcher's settings");
                Logger.GetInstance().Log(settings.api.company["global"].url);
                parsingSuccess = true;

            } catch (Exception e) {

                Logger.GetInstance().Error("Failed to parse and load launcher's settings", e);
                Dodoco.Application.Application.GetInstance().End(1);

            }

            //System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            //byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Hellow world"));
            //Logger.GetInstance().Debug(System.Convert.ToHexString(data));

        }

        private void WriteDefaultSettings() {

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

        }

        private void CreateThread() {

            Logger.GetInstance().Log("Creating launcher's window's thread...");

            this.windowThread = new Thread(() => {

                Logger.GetInstance().Log("Configuring launcher's window's...");

                this.window = new PhotinoNET.PhotinoWindow();

                this.window.SetLogVerbosity(0);
                this.window.SetTitle(Dodoco.Application.Application.GetInstance().title);
                this.window.SetUseOsDefaultLocation(true);
                this.window.SetUseOsDefaultSize(false);
                this.window.SetSize(new System.Drawing.Size(300, 400));
                this.window.Center();
                this.window.SetResizable(false);
                this.window.SetChromeless(false);
                this.window.RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) => {
                    contentType = "text/javascript";
                    return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(@"( () => window.location.reload(true) )();"));
                });
                // ---- For debugging ----
                //this.window.Load(new Uri($"http://localhost:5173/?id={(new Random().Next())}"));
                // -----------------------
                this.window.Load(new Uri($"http://localhost:{Dodoco.Application.Application.GetInstance().port}/?id={(new Random().Next())}"));

                this.window.RegisterWindowCreatingHandler(new EventHandler((object? sender, EventArgs e) => Logger.GetInstance().Log("Creating launcher's window...") ));
                this.window.RegisterWindowCreatedHandler(new EventHandler((object? sender, EventArgs e) => Logger.GetInstance().Log("Successfully created launcher's window") ));
                //this.window.RegisterWindowCreatedHandler(new EventHandler((object? sender, EventArgs e) => Task.Run(async () => { await Task.Delay(3000); this.SearchSettings(); return; }) ));
                this.window.RegisterWindowCreatedHandler(new EventHandler((object? sender, EventArgs e) => this.Main() ));
                this.window.RegisterWindowClosingHandler(new PhotinoNET.PhotinoWindow.NetClosingDelegate((object sender, EventArgs e) => {
            
                    Logger.GetInstance().Log("Closing launcher's window...");
                    this.window.Close();
                    Logger.GetInstance().Log("Successfully closed launcher's window");
                    
                    return false;

                }));

                Logger.GetInstance().Log("Successfully configured launcher's window's");

                this.window.WaitForClose();
                this.Finish();

            });

            this.windowThread.TrySetApartmentState(ApartmentState.STA);

            Logger.GetInstance().Log("Successfully created launcher's window's thread");

        }

        private async Task Main() {

            Logger.GetInstance().Log("STARTING MAIN");

            this.UpdateActivityState(LauncherActivityState.FETCHING_LAUNCHER_SETTINGS);
            this.SearchSettings();

            //LauncherSettings k = new LauncherSettings();

            Logger.GetInstance().Log("=========================================");
            Logger.GetInstance().Log(settings.api.company["global"].url);
            Logger.GetInstance().Log(settings.api.company["global"].key);
            Logger.GetInstance().Log("=========================================");

        }

        public void Open() {

            if (!this.IsRunning()) {

                Logger.GetInstance().Log("Starting launcher's window's thread...");

                this.windowThread?.Start();

                if (this.windowThread?.ThreadState == ThreadState.Running) {

                    Logger.GetInstance().Log("Successfully started launcher's window's thread");
                    this.UpdateExecutionState(LauncherExecutionState.RUNNING);

                } else {

                    Logger.GetInstance().Error("Failed to start launcher's window's thread");
                    this.Finish();

                }

            } else {

                Logger.GetInstance().Error("Launcher is already opened");

            }

        }

        public void Finish() {

            if (this.IsRunning()) {

                Logger.GetInstance().Log("Finishing launcher...");
                this.UpdateExecutionState(LauncherExecutionState.FINISHING);

                Logger.GetInstance().Log("Finishing launcher's window's thread asynchronously...");
                
                Task<bool>.Run(() => {

                    this.windowThread?.Join();
                    
                    return true;
                    

                }).ContinueWith((result) => {

                    Logger.GetInstance().Log("Successfully finished launcher's window's thread");
                    Logger.GetInstance().Log("Successfully finished launcher");
                    this.UpdateExecutionState(LauncherExecutionState.FINISHED);

                });

            } else {

                Logger.GetInstance().Error("Launcher is already finished");

            }

        }

        public bool IsRunning() {

            return executionState == LauncherExecutionState.RUNNING;

        }

        private void UpdateExecutionState(LauncherExecutionState newState) {

            Logger.GetInstance().Log($"Updating launcher's execution state from {executionState.ToString()} to {newState.ToString()}");
            executionState = newState;

        }

        private void UpdateActivityState(LauncherActivityState newState) {

            Logger.GetInstance().Log($"Updating launcher's activity state from {activityState.ToString()} to {newState.ToString()}");
            activityState = newState;

        }

    }

}