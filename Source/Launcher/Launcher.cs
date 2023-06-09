using Dodoco.Application;
using Dodoco.Game;
using Dodoco.Launcher.Cache;
using Dodoco.Launcher.Settings;
using Dodoco.Network;
using Dodoco.Network.Api.Company;
using Dodoco.Network.Api.Company.Launcher.Content;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Network.Api.Github.Repos;
using Dodoco.Util.Log;

using System.Drawing;

namespace Dodoco.Launcher {

    public class Launcher: ILauncher {

        private static Launcher? instance = null;
        private IApplicationWindow window;
        
        /*
         * Files
        */

        private LauncherCacheFile cacheFile = new LauncherCacheFile();
        private LauncherSettingsFile settingsFile = new LauncherSettingsFile();
        private ResourceCacheFile resourceCacheFile = new ResourceCacheFile();
        
        public LauncherActivityState ActivityState { get; private set; } = LauncherActivityState.UNREADY;
        public LauncherCache Cache { get; private set; } = new LauncherCache();
        public Content Content { get; private set; } = new Content();
        public LauncherExecutionState ExecutionState { get; private set; } = LauncherExecutionState.UNINITIALIZED;
        public IMutableGame? Game { get; private set; }
        public Resource Resource { get; private set; } = new Resource();
        public LauncherSettings Settings { get; private set; } = new LauncherSettings();

        public LauncherActivityState GetLauncherActivityState() => this.ActivityState;
        public LauncherCache GetLauncherCache() => this.Cache;
        public Content GetContent() => this.Content;
        public LauncherExecutionState GetLauncherExecutionState() => this.ExecutionState;
        public Resource GetResource() => this.Resource;
        public LauncherSettings GetLauncherSettings() => this.Settings;
        public IGame GetGame() => this.Game != null ? this.Game : throw new LauncherException("Game class not initialized yet");

        public void SetLauncherCache(LauncherCache Cache) {

            this.Cache = Cache;
            // Sync the file
            this.cacheFile.Content = this.Cache;
            this.cacheFile.WriteFile();

        }
        
        public void SetLauncherSettings(LauncherSettings newSettings) {

            this.Settings = newSettings;
            // Sync the file
            this.settingsFile.Content = this.Settings;
            this.settingsFile.WriteFile();

        }

        /*
         * Events
        */

        public event EventHandler<IGame> OnGameCreated = delegate {};
        public event EventHandler<int> OnOperationProgressChanged = delegate {};

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

        public void Start() {

            if (!this.IsRunning()) {

                this.window.SetTitle(Dodoco.Application.Application.GetInstance().title);
                this.window.SetSize(new Size(300, 400));
                this.window.SetResizable(false);
                this.window.SetFrameless(false);
                //this.window.SetUri(new Uri($"http://localhost:5173/?id={(new Random().Next())}"));
                // -----------------------
                this.window.SetUri(new Uri($"http://localhost:{Dodoco.Application.Application.GetInstance().port}"));
                
                this.window.OnOpen += new EventHandler((object? sender, EventArgs e) => this.Main().ConfigureAwait(false));
                this.window.OnClose += new EventHandler((object? sender, EventArgs e) => this.Stop());
                
                this.window.Open();

            } else {

                Logger.GetInstance().Error("Launcher is already running");

            }

        }

        private async Task Main() {

            this.UpdateExecutionState(LauncherExecutionState.RUNNING);

            try {

                /*
                 * Manages launcher's Settings file
                */

                this.UpdateActivityState(LauncherActivityState.FETCHING_LAUNCHER_SETTINGS);
                
                if (!this.settingsFile.Exists()) {

                    this.settingsFile.CreateFile();
                    this.settingsFile.Content = this.Settings;
                    this.settingsFile.WriteFile();

                }

                this.settingsFile.LoadFile();
                this.Settings = this.settingsFile.Content;

                /*
                 * Manages launcher's Cache file
                */

                this.UpdateActivityState(LauncherActivityState.FETCHING_LAUNCHER_CACHE);
                
                if (!this.cacheFile.Exists()) {

                    this.cacheFile.CreateFile();
                    this.cacheFile.Content = this.Cache;
                    this.cacheFile.WriteFile();

                }

                this.cacheFile.LoadFile();
                this.Cache = this.cacheFile.Content;

                /*
                 * Manages APIs
                */

                Logger.GetInstance().Log($"Fetching APIs...");
                this.UpdateActivityState(LauncherActivityState.FETCHING_WEB_DATA);

                CompanyApiFactory factory = new CompanyApiFactory(
                    this.Settings.api.company[this.Settings.game.server].url,
                    this.Settings.api.company[this.Settings.game.server].key,
                    this.Settings.api.company[this.Settings.game.server].launcher_id,
                    this.Settings.game.language
                );

                /*
                 * Manages Content API
                */

                try { this.Content = await factory.FetchLauncherContent(); }
                catch (NetworkException e) {

                    /* Since Content API it is not strictly
                     * necessary for launcher's work, the exception
                     * will be simply reported to the log.
                    */

                    Logger.GetInstance().Error(e.Message, e);

                }

                if (this.Content != null) {

                    if (this.Content.IsSuccessfull())
                        await this.Cache.UpdateFromContent(this.Content);
                    else
                        Logger.GetInstance().Error($"Failed to fetch Content API from remote servers (return code: {this.Content.retcode}, message: \"{this.Content.message}\")");

                }

                /*
                 * Manages Resource API and Resource cache file
                */
                
                this.Resource = await factory.FetchLauncherResource();
                if (!this.Resource.IsSuccessfull())
                    new LauncherException($"Failed to fetch Resource API from remote servers (return code: {this.Resource.retcode}, message: \"{this.Resource.message}\")");
                Version remoteGameVersion = Version.Parse(this.Resource.data.game.latest.version);
                
                if (this.resourceCacheFile.Exists()) {

                    this.resourceCacheFile.LoadFile();
                    this.Resource = this.resourceCacheFile.Content;

                } else {

                    this.resourceCacheFile.CreateFile();
                    this.resourceCacheFile.Content = this.Resource;
                    this.resourceCacheFile.WriteFile();

                }
                
                Logger.GetInstance().Log($"Finished fetching APIs");

                /*
                 * Manages game
                */

                if (!GameManager.CheckGameInstallation(this.Settings.game.installation_directory, this.Settings.game.server)) {

                    // TODO: download game
                    this.Game = GameManager.CreateGame(remoteGameVersion, this.Settings.game.server, this.Resource, this.Settings.game.installation_directory, GameState.WAITING_FOR_DOWNLOAD);
                    //await this.Game.Download(this.Resource, new DirectoryInfo("/home/neofox/.local/share/dodoco-launcher/"), CancellationToken.None);

                } else {

                    Version installedGameVersion = GameManager.SearchForGameVersion(this.Settings.game.installation_directory, this.Settings.game.server);

                    if (remoteGameVersion > installedGameVersion) {

                        // TODO: update game

                        Logger.GetInstance().Warning($"Current installed game version ({installedGameVersion}) is outdated, the newest game version is {remoteGameVersion}");

                        Resource oldVersionResource;

                        if (Version.Parse(resourceCacheFile.Content.data.game.latest.version) == installedGameVersion) {

                            oldVersionResource = resourceCacheFile.Content;

                        } else {

                            LauncherRepositoryApi repo = new LauncherRepositoryApi(this.Settings.api.Launcher);
                            oldVersionResource = await repo.FetchCachedLauncherResource(installedGameVersion, this.Settings.game.server);
                            this.resourceCacheFile.Content = oldVersionResource;
                            this.resourceCacheFile.WriteFile();

                        }

                        this.Game = GameManager.CreateGame(installedGameVersion, this.Settings.game.server, oldVersionResource, this.Settings.game.installation_directory, GameState.WAITING_FOR_UPDATE);

                    } else {

                        this.Game = GameManager.CreateGame(installedGameVersion, this.Settings.game.server, this.Resource, this.Settings.game.installation_directory, GameState.READY);

                    }

                }

                if (this.Game == null)
                    throw new LauncherException("Failed to create the game instance");

                this.OnGameCreated.Invoke(this, this.Game);

                this.window.SetSize(new Size(1270, 766));
                this.window.SetResizable(true);

            } catch (Exception e) {

                this.FatalStop(e);

            }

        }

        public void FatalStop(Exception e) {

            Logger.GetInstance().Error($"A fatal error occurred", e);
            this.Stop();

        }

        public void Stop() {

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

            return this.ExecutionState == LauncherExecutionState.RUNNING;

        }

        private void UpdateExecutionState(LauncherExecutionState newState) {

            Logger.GetInstance().Debug($"Updating launcher's execution state from {this.ExecutionState.ToString()} to {newState.ToString()}");
            this.ExecutionState = newState;

        }

        private void UpdateActivityState(LauncherActivityState newState) {

            Logger.GetInstance().Debug($"Updating launcher's activity state from {this.ActivityState.ToString()} to {newState.ToString()}");
            this.ActivityState = newState;

        }

        public async Task RepairGameFiles() {

            Logger.GetInstance().Debug($"1");

            if (this.Game == null) return;

            Logger.GetInstance().Debug($"2");

            try {

                List<GameFileIntegrityReport> mismatches = await this.Game.CheckFilesIntegrity();

                Logger.GetInstance().Debug($"3");

                foreach (var report in mismatches) {

                    switch (report.localFileIntegrityState) {

                        case GameFileIntegrityState.CORRUPTED:
                            Logger.GetInstance().Log($"Mismatch: the local file \"{report.localFilePath}\" MD5 hash ({report.localFileHash}) doesn't match the expected remote hash ({report.remoteFileHash})");
                            break;
                        
                        case GameFileIntegrityState.MISSING:
                            Logger.GetInstance().Log($"Mismatch: the local file \"{report.localFilePath}\" is missing");
                            break;

                    }

                }

                foreach (var report in mismatches) {

                    //await this.Game.RepairFile(report);

                }

            } catch (Exception e) {

                Logger.GetInstance().Debug($"4");

                throw new LauncherException($"Failed to repair game's files", e);
                
            }

        }

    }

}