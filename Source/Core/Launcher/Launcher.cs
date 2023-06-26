using Dodoco.Core.Embed;
using Dodoco.Core.Game;
using Dodoco.Core.Launcher.Cache;
using Dodoco.Core.Launcher.Settings;
using Dodoco.Core.Network;
using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Network.Api.Company.Launcher.Content;
using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Network.Api.Github.Repos;
using Dodoco.Core.Util.Log;
using Dodoco.Core.Wine;

namespace Dodoco.Core.Launcher {

    public class Launcher: ILauncher {

        private static Launcher? instance = null;
        
        /*
         * Files
        */

        private LauncherCacheFile cacheFile = new LauncherCacheFile();
        private LauncherSettingsFile settingsFile = new LauncherSettingsFile();
        
        public LauncherBackgroundImageFile BackgroundImageFile { get; private set; } = new LauncherBackgroundImageFile();
        public LauncherCache Cache { get; private set; } = new LauncherCache();
        public LauncherExecutionState ExecutionState { get; private set; } = LauncherExecutionState.UNINITIALIZED;
        public LauncherSettings Settings { get; private set; } = new LauncherSettings();
        public LauncherState State { get; private set; } = LauncherState.UNREADY;
        
        public Content Content { get; private set; } = new Content();
        public Resource Resource { get; private set; } = new Resource();
        
        public IGame? Game { get; private set; }
        public IWine? Wine { get; private set; }
        public IWinePackageManager? WinePackageManager { get; private set; }

        public LauncherCache GetLauncherCache() => this.Cache;
        public Content GetContent() => this.Content;
        public LauncherExecutionState GetLauncherExecutionState() => this.ExecutionState;
        public Resource GetResource() => this.Resource;
        public LauncherSettings GetLauncherSettings() => this.Settings;
        public IGame GetGame() => this.Game != null ? this.Game : throw new LauncherException("Game's instance not initialized yet");
        public IWine GetWine() => this.Wine != null ? this.Wine : throw new LauncherException("Wine's instance not initialized yet");

        public void UpdateLauncherCache(LauncherCache cache) {

            this.Cache = cache;
            this.cacheFile.Write(cache);

        }

        public void UpdateLauncherSettings(LauncherSettings settings) {

            this.Settings = settings;
            this.settingsFile.Write(settings);

        }

        /*
         * Events
        */

        public event EventHandler BeforeStart = delegate {};
        public event EventHandler AfterStart = delegate {};
        public event EventHandler<IWine> OnWineCreated = delegate {};
        public event EventHandler<IGame> OnGameCreated = delegate {};
        public event EventHandler<int> OnOperationProgressChanged = delegate {};

        private Launcher() {

            Logger.GetInstance().Log("Initializing launcher...");

        }

        public static async Task<ILauncher> Create() {

            ILauncher instance = new Launcher();
            await instance.Start();
            return instance;

        }

        public async Task Start() {

            this.BeforeStart.Invoke(this, EventArgs.Empty);

            try {

                /*
                 * Manages launcher's Settings file
                */

                this.UpdateState(LauncherState.FETCHING_LAUNCHER_SETTINGS);
                
                if (!this.settingsFile.Exist()) {

                    this.settingsFile.Create();
                    this.settingsFile.Write(this.Settings);

                }

                this.UpdateLauncherSettings(this.settingsFile.Read());

                /*
                 * Manages launcher's Cache file
                */
                
                if (!this.cacheFile.Exist()) {

                    this.cacheFile.Create();
                    this.cacheFile.Write(this.Cache);

                }

                /*
                 * Manages APIs
                */

                Logger.GetInstance().Log($"Fetching APIs...");
                this.UpdateState(LauncherState.FETCHING_WEB_DATA);

                CompanyApiFactory factory = new CompanyApiFactory(
                    this.Settings.Api.Company[this.Settings.Game.Server].Url,
                    this.Settings.Api.Company[this.Settings.Game.Server].Key,
                    this.Settings.Api.Company[this.Settings.Game.Server].LauncherId,
                    this.Settings.Game.Language
                );

                /*
                * Manages Content API
                */

                try { this.Content = await factory.FetchLauncherContent(); }
                catch (NetworkException e) {

                    /*
                     * Since Content API it is not strictly
                     * necessary for launcher's work, the exception
                     * will be simply reported to the log.
                    */

                    Logger.GetInstance().Error(e.Message, e);

                }

                if (this.Content != null) {

                    if (this.Content.IsSuccessfull()) {

                        if (!this.BackgroundImageFile.Exist()) {

                            this.BackgroundImageFile.Create();

                        }

                        await this.BackgroundImageFile.Update(this.Content);

                    } else {

                        Logger.GetInstance().Error($"Failed to fetch Content API from remote servers (return code: {this.Content.retcode}, message: \"{this.Content.message}\")");

                    }

                }

                /*
                * Manages Resource API and Resource cache file
                */
                
                this.Resource = await factory.FetchLauncherResource();
                if (!this.Resource.IsSuccessfull())
                    new LauncherException($"Failed to fetch Resource API from remote servers (return code: {this.Resource.retcode}, message: \"{this.Resource.message}\")");

                Logger.GetInstance().Log($"Finished fetching APIs");

                GitHubReposApiFactory wineRepository = new GitHubReposApiFactory(this.Settings.Api.Wine);
                this.WinePackageManager = new WineGeCustomPackageManager(this.Settings.Wine.ReleasesDirectory, wineRepository);

                /*
                 * Manages all launcher's resources
                */

                await this.ManageAllResources();

                this.AfterStart.Invoke(this, EventArgs.Empty);

            } catch (Exception e) {

                await this.FatalStop(e);

            }

        }

        public async Task ManageAllResources() {

            await this.ManageWine();
            await this.ManageGame();

        }

        public async Task ManageWine() {

            if (this.WinePackageManager == null) {

                Logger.GetInstance().Error("Wine's Package Manager instance not created");
                return;

            }

            /*
             * Manages Wine's instance
            */

            if (this.Settings.Wine.UserDefinedInstallation) {

                /*
                 * User-defined Wine installation
                */

                if (string.IsNullOrWhiteSpace(this.Settings.Wine.InstallationDirectory) || !Directory.Exists(this.Settings.Wine.InstallationDirectory)) {

                    throw new LauncherException("A Wine's installation directory must be supplied when using a user-defined Wine installation (UserDefinedInstallation = true)");

                }

                this.Wine = new Wine.Wine(this.Settings.Wine.InstallationDirectory, this.Settings.Wine.PrefixDirectory);

            } else {

                /*
                 * Launcher-defined Wine installation
                */

                if (string.IsNullOrWhiteSpace(this.Settings.Wine.SelectedRelease)) {

                    Logger.GetInstance().Warning($"The selected Wine's release is not present in the settings file. The default value will be used instead");
                    this.Settings.Wine.SelectedRelease = new LauncherSettings.WineConfig().SelectedRelease;
                    this.UpdateLauncherSettings(this.Settings);

                }

                this.Wine = this.WinePackageManager.GetWineFromTag(this.Settings.Wine.SelectedRelease, this.Settings.Wine.PrefixDirectory);

            }

            this.OnWineCreated.Invoke(this, this.Wine);

            if (this.Wine.State != WineState.READY) {

                Logger.GetInstance().Warning($"Wine is not ready yet");
                return;

            }

        }

        public async Task ManageGame() {

            if (this.Wine == null) {

                Logger.GetInstance().Warning($"Wine's instance is not ready yet, thus Game's instance will not be created");
                return;

            }

            /*
             * Manages Game's instance
            */

            Version remoteGameVersion = Version.Parse(this.Resource.data.game.latest.version);

            if (!GameManager.CheckGameInstallation(this.Settings.Game.InstallationDirectory, this.Settings.Game.Server)) {

                this.Game = GameManager.CreateGame(remoteGameVersion, this.Settings.Game.Server, this.Resource, this.Resource, this.Wine, this.Settings.Game.InstallationDirectory, GameState.WAITING_FOR_DOWNLOAD);

            } else {

                Version installedGameVersion = GameManager.SearchForGameVersion(this.Settings.Game.InstallationDirectory, this.Settings.Game.Server);

                if (remoteGameVersion > installedGameVersion) {

                    Logger.GetInstance().Warning($"Current installed game version ({installedGameVersion}) is outdated, the newest game version is {remoteGameVersion}");

                    Resource? oldVersionResource = null;

                    Logger.GetInstance().Log($"Searching for the {installedGameVersion.ToString()} version's resource...");

                    if (this.Cache.Resource.data.game.latest.version == installedGameVersion.ToString()) {

                        Logger.GetInstance().Log($"Found the {installedGameVersion.ToString()} version's resource in launcher's cache");
                        oldVersionResource = this.Cache.Resource;

                    } else {

                        oldVersionResource = EmbeddedResourceManager.GetLauncherResource(this.Settings.Game.Server, installedGameVersion);
                        Logger.GetInstance().Log($"Found the {installedGameVersion.ToString()} version's resource in launcher's embedded resources...");

                        this.Cache.Resource = oldVersionResource;
                        this.UpdateLauncherCache(this.Cache);

                    }

                    this.Game = GameManager.CreateGame(installedGameVersion, this.Settings.Game.Server, oldVersionResource, this.Resource, this.Wine, this.Settings.Game.InstallationDirectory, GameState.WAITING_FOR_UPDATE);

                } else {

                    Logger.GetInstance().Log($"Current installed game version ({installedGameVersion}) is updated");

                    if (this.Cache.Resource.data.game.latest.version != installedGameVersion.ToString()) {

                        this.Cache.Resource = this.Resource;
                        this.UpdateLauncherCache(this.Cache);

                    }

                    this.Game = GameManager.CreateGame(installedGameVersion, this.Settings.Game.Server, this.Resource, this.Resource, this.Wine, this.Settings.Game.InstallationDirectory, GameState.READY);

                }

            }

            if (this.Game == null)
                throw new LauncherException("Failed to create the Game's instance");

            this.OnGameCreated.Invoke(this, this.Game);

            if (this.Game.State != GameState.READY) {

                Logger.GetInstance().Warning($"Game is not ready yet");
                return;

            }

        }

        public async Task FatalStop(Exception e) {

            Logger.GetInstance().Error($"A fatal error occurred", e);
            await this.Stop();

        }

        public async Task Stop() {

            Logger.GetInstance().Log("Finishing launcher...");
            Logger.GetInstance().Log("Successfully finished launcher");

        }

        private void UpdateState(LauncherState newState) {

            Logger.GetInstance().Debug($"Updating launcher's state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;

        }

    }

}