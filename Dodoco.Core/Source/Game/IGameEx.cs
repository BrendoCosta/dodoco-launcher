namespace Dodoco.Core.Game;

using Dodoco.Core.Network.Api.Company;

public interface IGameEx {

    GameSettings Settings { get; set; }
    CompanyApiFactory ApiFactory { get; }

    /// <summary>
    /// Determines whether the game is installed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the game is installed in the directory set in the <see cref="P:Dodoco.Core.Game.GameSettings.InstallationDirectory"/> property; otherwise, <see langword="false"/>.
    /// </returns>
    bool CheckGameInstallation();

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

}