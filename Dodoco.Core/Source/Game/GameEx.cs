namespace Dodoco.Core.Game;

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

    public GameEx(GameSettings settings) {

        this.Settings = settings;

    }

    public CompanyApiFactory ApiFactory {
    
        get => new CompanyApiFactory(
            this.Settings.Api[this.Settings.Server].Url,
            this.Settings.Api[this.Settings.Server].Key,
            this.Settings.Api[this.Settings.Server].LauncherId,
            this.Settings.Language
        );

    }


    /// <summary>
    /// Determines whether the game is installed.
    /// </summary>
    /// <returns>
    /// True if the game is installed in the GameSettings.InstallationDirectory directory; otherwise, false.
    /// </returns>
    public virtual bool CheckGameInstallation() {

        return File.Exists(Path.Join(this.Settings.InstallationDirectory, this.GetMainExecutableName()));

    }

    /// <summary>
    /// Returns the name of game's data directory based on the GameSettings.Server value.
    /// </summary>
    /// <returns>
    /// The name of game's data directory.
    /// </returns>
    public virtual string GetDataDirectoryName() {

        return this.GetGameTitle() + "_Data";

    }

    /// <summary>
    /// Returns the internal game's title (used for data directory, main executable file and so on) based on the GameSettings.Server value.
    /// </summary>
    /// <returns>
    /// The the internal game's title.
    /// </returns>
    public virtual string GetGameTitle() {

       return this.Settings.Server == GameServer.Global ? "GenshinImpact" : "YuanShen";

    }

    /// <summary>
    /// Returns the game's main executable filename alongs its file extension based on the GameSettings.Server value.
    /// </summary>
    /// <returns>
    /// The the game's main executable name.
    /// </returns>
    public virtual string GetMainExecutableName() {

        return this.GetGameTitle() + ".exe";

    }

    /// <summary>
    /// Try to determine the game's version from the local installation files.
    /// If the game is not installed or the file doesn't exist, it will returns the latest 
    /// game version from resource API.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Version"/> object containing the the game's version.
    /// </returns>
    /// <exception cref="T:Dodoco.Core.Game.GameException">An error has occurred while fetching the remote server</exception>
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

                ResourceResponse resource = await this.ApiFactory.FetchLauncherResource();
                resource.EnsureSuccessStatusCode();
                gameVersion = Version.Parse(resource.data.game.latest.version);

            } catch (Exception e) {

                throw new GameException("Failed to get the latest game version from the server", e);

            }

        }

        Logger.GetInstance().Log($"Successfully found the game version \"{gameVersion.ToString()}\"");

        return gameVersion;

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

            Logger.GetInstance().Error($"The file \"{globalGameManagersPath}\" doesn't exist");

        }

        return null;

    }

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

            Logger.GetInstance().Error($"The file \"{unityPlayerPath}\" doesn't exist");

        }

        return null;

    }
    
}