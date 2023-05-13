using Dodoco.Network.Api.Company;
using Dodoco.Network.Api.Company.Launcher;
using Dodoco.Application;
using Dodoco.Network;
using Dodoco.Network.Controller;
using Dodoco.Game;
using Dodoco.Util;
using Dodoco.Util.Log;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Launcher {

    public class Launcher {

        private static Launcher? instance = null;

        // https://github.com/Raxdiam/photino.API

        private IApplicationWindow window;
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
            
            this.window = new ApplicationWindow();

        }

        public void Run() {

            if (!this.IsRunning()) {

                this.window.SetTitle(Dodoco.Application.Application.GetInstance().title);
                this.window.SetSize(new System.Drawing.Size(300, 400));
                this.window.SetResizable(false);
                this.window.SetFrameless(false);
                this.window.SetUri(new Uri($"http://localhost:5173/?id={(new Random().Next())}"));
                // -----------------------
                //this.window.SetUri(new Uri($"http://localhost:{Dodoco.Application.Application.GetInstance().port}/?id={(new Random().Next())}"));
                
                this.window.OnOpen += new EventHandler(async (object? sender, EventArgs e) => this.Main());
                this.window.OnClose += new EventHandler((object? sender, EventArgs e) => this.Finish());
                
                this.window.Open();

            } else {

                Logger.GetInstance().Error("Launcher is already running");

            }

        }

        private async Task Main() {

            this.UpdateExecutionState(LauncherExecutionState.RUNNING);

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

                try { content = await factory.FetchLauncherContent(); }
                catch (NetworkException e) {

                    /* Since content API it is not strictly
                     * necessary for launcher's work, the exception
                     * will be simply reported to the log.
                    */

                    Logger.GetInstance().Error(e.Message, e);

                }

                if (content != null) {

                    if (content.IsSuccessfull())
                        await cache.UpdateFromContent(content);
                    else
                        Logger.GetInstance().Error($"Failed to fetch content API from remote servers (return code: {content.retcode}, message: \"{content.message}\")");

                }

                /*
                 * Manages resource API
                */
                
                resource = await factory.FetchLauncherResource();
                if (resource.IsSuccessfull())
                    Game.Stable.Game.GetInstance().SetVersion(Version.Parse(resource.data.game.latest.version));
                else
                    new LauncherException($"Failed to fetch resource API from remote servers (return code: {resource.retcode}, message: \"{resource.message}\")");

                Logger.GetInstance().Log($"Finished fetching APIs");

                /*
                 * Manages game
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

            } catch (Exception e) {

                Logger.GetInstance().Error($"A fatal error occurred", e);

            }

        }

        public void Finish() {

            if (this.IsRunning()) {

                Logger.GetInstance().Log("Finishing launcher...");
                this.UpdateExecutionState(LauncherExecutionState.FINISHING);
                if (this.window.IsOpen())
                    this.window.Close();
                Logger.GetInstance().Log("Successfully finished launcher");

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
            ServerSentEvents.PushEvent(new Dodoco.Network.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = newState.ToString()
            });

        }

        private void UpdateActivityState(LauncherActivityState newState) {

            Logger.GetInstance().Debug($"Updating launcher's activity state from {activityState.ToString()} to {newState.ToString()}");
            activityState = newState;
            ServerSentEvents.PushEvent(new Dodoco.Network.HTTP.SSE.Event() {
                eventName = Reflection.GetCurrentMethod(),
                data = newState.ToString()
            });

        }

    }

}