namespace Dodoco.Core.Game;

using Dodoco.Core.Embed;
using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Util.FileSystem;
using Dodoco.Core.Util.Log;

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class GameEx: IGameEx {

    public GameSettings Settings { get; set; }
    protected GameResourceCacheFile ResourceCacheFile = new GameResourceCacheFile();

    public GameEx(GameSettings settings) {

        this.Settings = settings;
        
        if (!this.ResourceCacheFile.Exist()) {

            this.ResourceCacheFile.Create();
            this.ResourceCacheFile.Write(new List<GameResourceCache>());

        }

    }

    /// <inheritdoc />
    public virtual bool CheckGameInstallation() {

        return File.Exists(Path.Join(this.Settings.InstallationDirectory, this.GetMainExecutableName()));

    }

    /// <inheritdoc />
    public virtual CompanyApiFactory GetApiFactory() => new CompanyApiFactory(
        
        this.Settings.Api[this.Settings.Server].Url,
        this.Settings.Api[this.Settings.Server].Key,
        this.Settings.Api[this.Settings.Server].LauncherId,
        this.Settings.Language

    );

    /// <inheritdoc />
    public virtual string GetDataDirectoryName() {

        return this.GetGameTitle() + "_Data";

    }

    /// <inheritdoc />
    public virtual string GetGameTitle() {

       return this.Settings.Server == GameServer.Global ? "GenshinImpact" : "YuanShen";

    }

    /// <inheritdoc />
    public virtual string GetMainExecutableName() {

        return this.GetGameTitle() + ".exe";

    }

    /// <inheritdoc />
    public virtual async Task<Version> GetGameVersionAsync() {

        Logger.GetInstance().Log($"Trying to find the game version...");

        Version? gameVersion = null;

        try {

            gameVersion = this.GetGameVersionFromGlobalGameManagers() ?? this.GetGameVersionFromUnityPlayer();

        } catch {

            Logger.GetInstance().Warning($"Unable to find the version from game's files, trying to find the latest game version from the server...");

        }

        if (gameVersion == null) {

            try {

                ResourceResponse resource = await this.GetApiFactory().FetchLauncherResource();
                resource.EnsureSuccessStatusCode();
                gameVersion = Version.Parse(resource.data.game.latest.version);

            } catch (Exception e) {

                throw new GameException("Failed to get the latest game version from the server", e);

            }

        }

        Logger.GetInstance().Log($"Successfully found the game version \"{gameVersion.ToString()}\"");

        return gameVersion;

    }

    /// <inheritdoc />
    public virtual async Task<ResourceResponse> GetResourceAsync() {

        Logger.GetInstance().Log($"Trying to find the resource data for current game version ({(await this.GetGameVersionAsync()).ToString()})...");
        
        if (!this.CheckGameInstallation()) {

            // If the game is not installed, return the latest resource from the server

            ResourceResponse remoteResource = await this.GetApiFactory().FetchLauncherResource();
            remoteResource.EnsureSuccessStatusCode();

            Logger.GetInstance().Log($"Game is not installed. Successfully returned the resource from remote server");

            return remoteResource;

        } else {

            ResourceResponse remoteResource = await this.GetApiFactory().FetchLauncherResource();
            remoteResource.EnsureSuccessStatusCode();

            Version installedGameVersion = await this.GetGameVersionAsync();
            Version remoteGameVersion = Version.Parse(remoteResource.data.game.latest.version);
            Predicate<GameResourceCache> desiredGameResourceCache = (GameResourceCache x) => {
                return x.Server == this.Settings.Server && Version.Parse(x.Resource.data.game.latest.version) == installedGameVersion;
            };

            if (installedGameVersion == remoteGameVersion) {

                // Game is updated

                Logger.GetInstance().Log($"Successfully returned the resource from remote server");
                return remoteResource;

            } else if (installedGameVersion < remoteGameVersion) {

                /*
                 * Game is outdated, first it should try to find
                 * the previous version's resource saved in the cache
                */

                Logger.GetInstance().Warning($"The current installed game is outdated. The installed version is \"{installedGameVersion.ToString()}\" while the latest one is \"{remoteGameVersion.ToString()}\"");

                ResourceResponse oldVersionResource;
                
                if (this.ResourceCacheFile.Exist() && this.ResourceCacheFile.Read().Exists(desiredGameResourceCache)) {

                    // If the resource exist in the cache then we return it

                    oldVersionResource = this.ResourceCacheFile.Read().Find(desiredGameResourceCache).Resource;
                    Logger.GetInstance().Log($"Successfully returned the resource from local cache");

                } else {

                    // Otherwise we hope to find it in embedded resources

                    oldVersionResource = EmbeddedResourceManager.GetLauncherResource(this.Settings.Server, installedGameVersion);
                    Logger.GetInstance().Log($"Successfully returned the resource from embedded resources");

                }

                return oldVersionResource;

            } else {

                throw new GameException($"The installed version ({installedGameVersion.ToString()}) can't be greater than the returned by the server ({remoteGameVersion.ToString()})");

            }

        }

    }

    /// <inheritdoc />
    public virtual async Task<ResourceGame?> GetGameUpdateAsync() {

        if (!this.CheckGameInstallation())
            return null;

        ResourceResponse latestResource = await this.GetApiFactory().FetchLauncherResource();
        latestResource.EnsureSuccessStatusCode();

        if (Version.Parse(latestResource.data.game.latest.version) > await this.GetGameVersionAsync())
            return latestResource.data.game;

        return null;

    }

    /// <inheritdoc />
    public virtual async Task UpdateGameResourceCacheAsync() {

        List<GameResourceCache> list = new List<GameResourceCache>();
        
        if (!this.ResourceCacheFile.Exist()) {

            this.ResourceCacheFile.Create();
            this.ResourceCacheFile.Write(new List<GameResourceCache>());

        }

        list = this.ResourceCacheFile.Read();
        Version gameVersion = await this.GetGameVersionAsync();

        if (!list.Exists(e => (e.Server == this.Settings.Server && Version.Parse(e.Resource.data.game.latest.version) == gameVersion))) {

            list.Add(new GameResourceCache {

                Server = this.Settings.Server,
                Resource = await this.GetResourceAsync()

            });

            this.ResourceCacheFile.Write(list);

        }

    }

    /// <summary>
    /// Try to find the game version from /*_Data/globalgamemanagers file with the regex
    /// pattern "^([1-9]+\.[0-9]+\.[0-9]+)_[\d]+_[\d]+".
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Version" /> object containing the the game's version
    /// if it was found inside the file; otherwise <see langword="null"/>.
    /// </returns>
    protected virtual Version? GetGameVersionFromGlobalGameManagers() {

        string globalGameManagersPath = Path.Join(this.Settings.InstallationDirectory, this.GetDataDirectoryName(), "globalgamemanagers");

        Logger.GetInstance().Log($"Looking for the game version inside \"{globalGameManagersPath}\" file...");

        if (File.Exists(globalGameManagersPath)) {

            using (Stream fs = File.OpenRead(globalGameManagersPath)) {

                const int BUFFER_SIZE = 23 * (int) DataUnit.BYTE;
                byte[] buffer = new byte[BUFFER_SIZE];
                string text = string.Empty;

                for (long offset = 0; offset < fs.Length; offset++) {

                    if (fs.Read(buffer, 0, BUFFER_SIZE) != 0) {

                        text = Encoding.ASCII.GetString(buffer);

                        if (Regex.IsMatch(text, @"^([1-9]+\.[0-9]+\.[0-9]+)_[\d]+_[\d]+")) {

                            Version ver = Version.Parse(text.Split("_")[0]);
                            return ver;

                        } else {

                            fs.Seek(offset, SeekOrigin.Begin);

                        }

                    }

                }

            }

        } else {

            Logger.GetInstance().Warning($"The file \"{globalGameManagersPath}\" doesn't exist");

        }

        return null;

    }

    /// <summary>
    /// Try to find the game version from /UnityPlayer.dll file with the regex
    /// pattern "^(OSRELWin)([\d]+[.][\d]+[.][\d]+)".
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Version" /> object containing the the game's version
    /// if it was found inside the file; otherwise <see langword="null"/>.
    /// </returns>
    protected virtual Version? GetGameVersionFromUnityPlayer() {

        string unityPlayerPath = Path.Join(this.Settings.InstallationDirectory, "UnityPlayer.dll");

        Logger.GetInstance().Log($"Looking for the game version inside \"{unityPlayerPath}\" file...");

        if (File.Exists(unityPlayerPath)) {

            using (Stream fs = File.OpenRead(unityPlayerPath)) {

                const int BUFFER_SIZE = 13 * (int) DataUnit.BYTE;
                byte[] buffer = new byte[BUFFER_SIZE];
                string text = string.Empty;

                for (long offset = 0; offset < fs.Length; offset++) {

                    if (fs.Read(buffer, 0, BUFFER_SIZE) != 0) {

                        text = Encoding.ASCII.GetString(buffer);

                        if (Regex.IsMatch(text, @"^(OSRELWin)([\d]+[.][\d]+[.][\d]+)")) {

                            Version ver = Version.Parse(text.Substring(8));
                            return ver;

                        } else {

                            fs.Seek(offset, SeekOrigin.Begin);

                        }

                    }

                }

            }

        } else {

            Logger.GetInstance().Warning($"The file \"{unityPlayerPath}\" doesn't exist");

        }

        return null;

    }
    
}