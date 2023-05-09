using Dodoco.Api.Company;
using Dodoco.Api.Company.Launcher;
using Dodoco.Application;
using Dodoco.Controller;
using Dodoco.Game;
using Dodoco.Util;
using Dodoco.Util.Log;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Launcher {

    public class Launcher {

        private static Launcher? instance = null;

        // https://github.com/Raxdiam/photino.API

        private PhotinoNET.PhotinoWindow? window = null;
        private Thread? windowThread;
        public static LauncherExecutionState executionState { get; private set; } = LauncherExecutionState.UNINITIALIZED;
        public static LauncherActivityState activityState { get; private set; } = LauncherActivityState.UNREADY;
        
        /*
         * Launcher's files managed by Main() method
        */

        public LauncherSettings settings = new LauncherSettings();
        public LauncherCache cache = new LauncherCache();
        
        public Content? content { get; private set; }
        public Resource? resource { get; private set; }
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

            this.UpdateExecutionState(LauncherExecutionState.INITIALIZING);
            this.CreateThread();

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
                this.window.Load(new Uri($"http://localhost:5173/?id={(new Random().Next())}"));
                // -----------------------
                //this.window.Load(new Uri($"http://localhost:{Dodoco.Application.Application.GetInstance().port}/?id={(new Random().Next())}"));

                this.window.RegisterWindowCreatingHandler(new EventHandler((object? sender, EventArgs e) => Logger.GetInstance().Log("Creating launcher's window...") ));
                this.window.RegisterWindowCreatedHandler(new EventHandler((object? sender, EventArgs e) => Logger.GetInstance().Log("Successfully created launcher's window") ));
                //this.window.RegisterWindowCreatedHandler(new EventHandler((object? sender, EventArgs e) => Task.Run(async () => { await Task.Delay(3000); this.SearchSettings(); return; }) ));
                this.window.RegisterWindowCreatedHandler(new EventHandler(async (object? sender, EventArgs e) => await this.Main() ));
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

            try {

                /*
                 * Manages launcher's settings file
                */

                this.UpdateActivityState(LauncherActivityState.FETCHING_LAUNCHER_SETTINGS);
                
                if (!this.settings.Exists()) {

                    this.settings.CreateFile();
                    this.settings.WriteFile();

                }

                this.settings = this.settings.LoadFile();

                /*
                 * Manages launcher's cache file
                */

                this.UpdateActivityState(LauncherActivityState.FETCHING_LAUNCHER_CACHE);
                
                if (!cache.Exists()) {

                    cache.CreateFile();
                    cache.WriteFile();

                }

                cache = cache.LoadFile();

                /*
                 * Manages APIs
                */

                Logger.GetInstance().Log($"Fetching APIs...");
                this.UpdateActivityState(LauncherActivityState.FETCHING_WEB_DATA);

                CompanyApiFactory factory = new CompanyApiFactory(
                    this.settings.api.company[this.settings.game.server].url,
                    this.settings.api.company[this.settings.game.server].key,
                    this.settings.api.company[this.settings.game.server].launcher_id,
                    this.settings.game.language
                );

                /*
                 * Manages content API
                */

                content = await factory.FetchLauncherContent();
                if (content != null && content.IsSuccessfull())
                    await cache.UpdateFromContent(content);

                /*
                 * Manages resource API
                */
                
                resource = await factory.FetchLauncherResource();
                if (resource != null && resource.IsSuccessfull())
                    Game.Stable.Game.GetInstance().SetVersion(Version.Parse(resource.data.game.latest.version));

                Logger.GetInstance().Log($"Finished fetching APIs");

                /*
                 *
                */

                if (!GameManager.CheckGameInstallation(this.settings.game.installation_path, this.settings.game.server)) {

                    // TODO: download game
                    this.UpdateActivityState(LauncherActivityState.NEEDS_GAME_DOWNLOAD);

                } else {

                    Version installedGameVersion = GameManager.SearchForGameVersion(this.settings.game.installation_path, this.settings.game.server);
                    Version remoteGameVersion = Version.Parse(resource.data.game.latest.version);

                    if (remoteGameVersion > installedGameVersion) {

                        // TODO: update game
                        this.UpdateActivityState(LauncherActivityState.NEEDS_GAME_UPDATE);

                    } else {

                        this.UpdateActivityState(LauncherActivityState.READY_TO_PLAY);

                    }

                    Game.IGame game = GameManager.CreateFromVersion(installedGameVersion);

                }

                await Task.Run(async () => {

                    await Task.Delay(5000);

                    Logger.GetInstance().Log($"Test");


                });

            } catch (Exception e) {

                Logger.GetInstance().Error($"A fatal erro occurred", e);

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

            Logger.GetInstance().Debug($"Updating launcher's execution state from {executionState.ToString()} to {newState.ToString()}");
            executionState = newState;
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = newState.ToString()
            });

        }

        private void UpdateActivityState(LauncherActivityState newState) {

            Logger.GetInstance().Debug($"Updating launcher's activity state from {activityState.ToString()} to {newState.ToString()}");
            activityState = newState;
            ServerSentEvents.PushEvent(new Dodoco.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = newState.ToString()
            });

        }

    }

}