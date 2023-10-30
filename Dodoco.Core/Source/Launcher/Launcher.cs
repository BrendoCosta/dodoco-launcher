using Dodoco.Core.Embed;
using Dodoco.Core.Game;
using Dodoco.Core.Launcher.Cache;
using Dodoco.Core.Launcher.Settings;
using Dodoco.Core.Network;
using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Protocol.Company.Launcher.Content;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
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
        
        private LauncherCache _Cache = new LauncherCache();
        public LauncherCache Cache {

            get => this._Cache;

            set {

                this._Cache = value;
                if (!this.cacheFile.Exist())
                    this.cacheFile.Create();
                    this.cacheFile.Write(value);

            }

        }
        
        private LauncherSettings _Settings = new LauncherSettings();
        public LauncherSettings Settings {

            get => this._Settings;

            set {

                this._Settings = value;
                if (!this.settingsFile.Exist())
                    this.settingsFile.Create();
                    this.settingsFile.Write(value);

                if (this.Resource.IsSuccessfull())
                    this.CheckDependencies();

            }

        }

        public LauncherState State { get; private set; } = LauncherState.UNREADY;
        public LauncherDependency Dependency { get; private set; } = LauncherDependency.NONE;
        
        public ContentResponse Content { get; private set; } = new ContentResponse();
        public ResourceResponse Resource { get; private set; } = new ResourceResponse();
        
        public IGame? Game { get; private set; }
        public IWine? Wine { get; private set; }
        public IWinePackageManager? WinePackageManager { get; private set; }

        public void UpdateLauncherCache(LauncherCache cache) {

            this.Cache = cache;
            this.cacheFile.Write(cache);

        }

        /*
         * Events
        */

        public event EventHandler<LauncherState> OnStateUpdate = delegate {};
        public event EventHandler<LauncherDependency> OnDependenciesUpdated = delegate {};

        public Launcher() {

            Logger.GetInstance().Log("Initializing launcher...");

        }

        public async Task Start() {

            try {

                /*
                 * Manages launcher's Settings file
                */
                
                if (!this.settingsFile.Exist()) {

                    this.settingsFile.Create();
                    this.settingsFile.Write(this.Settings);

                }

                this.Settings = this.settingsFile.Read();

                /*
                 * Manages launcher's Cache file
                */
                
                if (!this.cacheFile.Exist()) {

                    this.cacheFile.Create();
                    this.cacheFile.Write(this.Cache);

                }

                this.Cache = this.cacheFile.Read();

                /*
                 * Manages APIs
                */

                Logger.GetInstance().Log($"Fetching APIs...");
                this.UpdateState(LauncherState.FETCHING_WEB_DATA);

                CompanyApiFactory factory = new CompanyApiFactory(
                    this.Settings.Game.Api[this.Settings.Game.Server].Url,
                    this.Settings.Game.Api[this.Settings.Game.Server].Key,
                    this.Settings.Game.Api[this.Settings.Game.Server].LauncherId,
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
                this.WinePackageManager.AfterReleaseDownload += (object? sender, EventArgs e) => this.CheckDependencies();

                /*
                 * Updates all launcher's resources
                */

                this.OnDependenciesUpdated += (object? sender, LauncherDependency e) => {

                    if (e == LauncherDependency.NONE
                    || e == LauncherDependency.GAME_UPDATE
                    || e == LauncherDependency.GAME_DOWNLOAD)
                        this.CreateInstances();

                };

                this.CheckDependencies();
                this.UpdateState(LauncherState.READY);

            } catch (Exception e) {

                this.Stop(e);

            }

        }

        public void CreateInstances() {

            //this.Dxvk = new Dxvk(Path.Join(this.Settings.Wine.DxvkConfig.ReleasesDirectory, "dxvk-2.2"));
            //this.Dxvk.InstallToDirectory(this.Settings.Game.InstallationDirectory);

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

                }

                this.Wine = this.WinePackageManager.GetWineFromTag(this.Settings.Wine.SelectedRelease, this.Settings.Wine.PrefixDirectory);

            }

            /*
             * Manages Game's instance
            */

            Version remoteGameVersion = Version.Parse(this.Resource.data.game.latest.version);

            if (!GameInstallationManager_Old.CheckGameInstallation(this.Settings.Game.InstallationDirectory, this.Settings.Game.Server)) {

                this.Game = GameInstallationManager_Old.CreateGame(remoteGameVersion, this.Settings.Game, this.Resource);

            } else {

                Version installedGameVersion = GameInstallationManager_Old.SearchForGameVersion(this.Settings.Game.InstallationDirectory, this.Settings.Game.Server);

                if (remoteGameVersion > installedGameVersion) {

                    Logger.GetInstance().Warning($"Current installed game version ({installedGameVersion}) is outdated, the newest game version is {remoteGameVersion}");

                    ResourceResponse? oldVersionResource = null;

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

                    this.Game = GameInstallationManager_Old.CreateGame(installedGameVersion, this.Settings.Game, oldVersionResource);

                } else {

                    Logger.GetInstance().Log($"Current installed game version ({installedGameVersion}) is updated");

                    if (this.Cache.Resource.data.game.latest.version != installedGameVersion.ToString()) {

                        this.Cache.Resource = this.Resource;
                        this.UpdateLauncherCache(this.Cache);

                    }

                    this.Game = GameInstallationManager_Old.CreateGame(remoteGameVersion, this.Settings.Game, this.Resource);

                }

            }

            this.Game.Wine = this.Wine;
            this.Game.AfterGameDownload += (object? sender, EventArgs e) => this.CheckDependencies();
            this.Game.AfterGameUpdate += (object? sender, EventArgs e) => this.CheckDependencies();

            return;

        }

        public void Stop(Exception? e = null) {

            Logger.GetInstance().Log("Finishing launcher...");
            
            if (e != null) {

                Logger.GetInstance().Error($"A fatal error occurred", e);

            } else {

                Logger.GetInstance().Log("Successfully finished launcher");

            }

        }

        private void UpdateState(LauncherState newState) {

            Logger.GetInstance().Debug($"Updating launcher's state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;
            this.OnStateUpdate.Invoke(this, newState);

        }

        private void UpdateDependency(LauncherDependency newDependency) {

            Logger.GetInstance().Debug($"Updating launcher's dependencies from {this.Dependency.ToString()} to {newDependency.ToString()}");
            this.Dependency = newDependency;
            this.OnDependenciesUpdated.Invoke(this, newDependency);

        }

        private void CheckDependencies() {

            Logger.GetInstance().Log($"Checking launcher's dependencies...");

            /*
             * Wine's dependencies
            */

            if (this.Settings.Wine.UserDefinedInstallation) {

                /*
                 * User-defined Wine installation
                */

                if (string.IsNullOrWhiteSpace(this.Settings.Wine.InstallationDirectory) || !Directory.Exists(this.Settings.Wine.InstallationDirectory)) {

                    this.UpdateDependency(LauncherDependency.WINE_CONFIGURATION);
                    return;

                }

            } else {

                /*
                 * Launcher-defined Wine installation
                */

                if (this.WinePackageManager == null) {

                    this.UpdateDependency(LauncherDependency.WINE_CONFIGURATION);
                    return;

                } else {

                    if (this.WinePackageManager.GetInstalledTags().Count == 0) {

                        this.UpdateDependency(LauncherDependency.WINE_DOWNLOAD);
                        return;

                    }

                }

            }

            /*
             * Game's dependencies
            */

            Version remoteGameVersion = Version.Parse(this.Resource.data.game.latest.version);

            if (!GameInstallationManager_Old.CheckGameInstallation(this.Settings.Game.InstallationDirectory, this.Settings.Game.Server)) {

                this.UpdateDependency(LauncherDependency.GAME_DOWNLOAD);
                return;

            } else {

                Version installedGameVersion = GameInstallationManager_Old.SearchForGameVersion(this.Settings.Game.InstallationDirectory, this.Settings.Game.Server);

                if (remoteGameVersion > installedGameVersion) {

                    this.UpdateDependency(LauncherDependency.GAME_UPDATE);
                    return;

                }

            }

            this.UpdateDependency(LauncherDependency.NONE);
            return;

        }

    }

}