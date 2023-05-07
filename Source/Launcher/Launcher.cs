using Dodoco.Util.Log;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dodoco.Application;
using Dodoco.Api.Company.Launcher.Resource;

namespace Dodoco.Launcher {

    public class Launcher {

        private static Launcher? instance = null;

        // https://github.com/Raxdiam/photino.API

        private PhotinoNET.PhotinoWindow? window = null;
        private Thread? windowThread;
        public static LauncherExecutionState executionState { get; private set; } = LauncherExecutionState.UNINITIALIZED;
        public static LauncherActivityState activityState { get; private set; } = LauncherActivityState.UNREADY;
        public static LauncherSettings settings = new LauncherSettings();
        public Resource launcherResource { get; private set; } = new Resource();
        public static JsonSerializerOptions jsonOptions = new JsonSerializerOptions() {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

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

        private void ManageSettings() {

            if (!settings.SearchSettingsFile())
                if (!settings.WriteDefaultSettings())
                    return;

            settings = settings.LoadSettings();

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
            this.ManageSettings();
            this.UpdateActivityState(LauncherActivityState.FETCHING_WEB_DATA);
            await this.FetchResource();

            // @TODO: compare fetched version with installed game's version

        }

        private async Task FetchResource() {

            string urlToFetch = $"{settings.api.company[settings.game.server].url}/resource?key={settings.api.company[settings.game.server].key}&launcher_id={settings.api.company[settings.game.server].launcher_id}";
            Logger.GetInstance().Log($"Trying to fetch latest game's launcher's resource data from remote servers ({urlToFetch})...");
            
            HttpResponseMessage response = await Application.Application.GetInstance().client.GetAsync(urlToFetch);
            
            if (response.IsSuccessStatusCode) {

                Logger.GetInstance().Log("Successfully fetch latest game's launcher's resource data from remote servers");

                Logger.GetInstance().Log("Trying to parse the received data...");
                try { launcherResource = JsonSerializer.Deserialize<Resource>(await response.Content.ReadAsStringAsync(), jsonOptions); }
                catch (JsonException e) { Logger.GetInstance().Error("Failed to parse latest game's launcher's resource data. Maybe the API has been changed.", e); }
                Logger.GetInstance().Log("Successfully parsed the received data");

            } else {

                Logger.GetInstance().Error($"Failed to fetch latest game's launcher's resource data from remote servers (received HTTP status code {response.StatusCode})");

            }

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