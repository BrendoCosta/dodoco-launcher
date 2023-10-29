namespace Dodoco.Core.Game;

using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Protocol.Company.Launcher.Resource;

public interface IGameEx {

    GameSettings Settings { get; set; }

    /// <summary>
    /// Determines whether the game is installed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the game is installed in the directory set in the <see cref="P:Dodoco.Core.Game.GameSettings.InstallationDirectory"/> property; otherwise, <see langword="false"/>.
    /// </returns>
    bool CheckGameInstallation();

    /// <returns>
    /// Returns the <see cref="T:Dodoco.Core.Network.Api.Company.CompanyApiFactory"/> used
    /// by the class to fetch the server's APIs.
    /// </returns>
    CompanyApiFactory GetApiFactory();

    /// <summary>
    /// Returns the name of game's data directory based on the current <see cref="P:Dodoco.Core.Game.GameSettings.Server"/> property's value.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> containing the name of game's data directory.
    /// </returns>
    string GetDataDirectoryName();

    /// <summary>
    /// Returns the internal game's title (used for data directory, main executable file and so on)
    /// based on the current <see cref="P:Dodoco.Core.Game.GameSettings.Server"/> property's value.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> containing internal game's title.
    /// </returns>
    string GetGameTitle();

    /// <summary>
    /// Returns the game's main executable filename alongs its file extension based on the <see cref="P:Dodoco.Core.Game.GameSettings.Server"/> property's value.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> containing the the game's main executable name alongs its file extension.
    /// </returns>
    string GetMainExecutableName();

    /// <summary>
    /// Try to determine the game's version from the local installation files.
    /// If the game is not installed or the file doesn't exist, it will returns the latest 
    /// game version from resource API.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Version"/> object containing the the game's version.
    /// </returns>
    /// <exception cref="T:Dodoco.Core.Game.GameException">An error has occurred while fetching the remote server</exception>
    Task<Version> GetGameVersionAsync();

    /// <summary>
    /// Returns the <see cref="T:Dodoco.Core.Protocol.Company.Launcher.Resource.ResourceResponse"/>
    /// object used by current game version. This object can be obtained through the remote server when
    /// the game is either not installed or updated, by the <see cref="T:Dodoco.Core.Game.GameResourceCacheFile"/>
    /// file or by the library's build-time embedded resources when the game is outdated.
    /// </summary>
    /// <returns>
    /// Returns the <see cref="T:Dodoco.Core.Protocol.Company.Launcher.Resource.ResourceResponse"/>
    /// object used by current game version.
    /// </returns>
    Task<ResourceResponse> GetResourceAsync();

    /// <summary>
    /// This method updates the content of the <see cref="T:Dodoco.Core.Game.GameResourceCacheFile"/> file to ensure that an
    /// <see cref="T:Dodoco.Core.Protocol.Company.Launcher.Resource.ResourceResponse"/> for the previous game version
    /// will be available when the game receives a later update.
    /// </summary>
    Task UpdateGameResourceCacheAsync();

}