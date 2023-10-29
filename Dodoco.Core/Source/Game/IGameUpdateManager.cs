namespace Dodoco.Core.Game;

using Dodoco.Core.Protocol.Company.Launcher.Resource;

public interface IGameUpdateManager: IStatefulEntity<GameUpdateManagerState> {

    /// <returns>
    /// Returns the <see cref="T:Dodoco.Core.Protocol.Company.Launcher.Resource.ResourceGame"/>
    /// object from the game's pre-update if it is available; otherwise returns <see langword="null"/>.
    /// </returns>
    Task<ResourceGame?> GetGamePreUpdateAsync();

    /// <summary>
    /// Verifies if the current game is updated.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="T:Dodoco.Core.Protocol.Company.Launcher.Resource.ResourceResponse"/>
    /// object if there is a game update; otherwise <see langword="null"/>.
    /// </returns>
    Task<ResourceGame?> GetGameUpdateAsync();

    /// <summary>
    /// Verifies if the game's pre-update package is downloaded in the game's
    /// installation directory. NOTICE: this method doesn't compare the actual
    /// package checksum with the one returned by the server.
    /// </summary>
    /// <returns>
    /// Returns <see langword="true"/> if the pre-update's package is in the
    /// game's installation directory; otherwise returns <see langword="false"/>.
    /// </returns>
    Task<bool> IsGamePreUpdateDownloadedAsync();
    
    /// <summary>
    /// Downloads the pre-update package to the game installation directory in order to save time.
    /// NOTICE: this method doesn't update the game, see <see cref="UpdateGameAsync(Dodoco.Core.ProgressReporter{ProgressReport}, System.Threading.CancellationToken)"/>
    /// as an alternative to updating.
    /// </summary>
    /// <seealso cref="UpdateGameAsync(Dodoco.Core.ProgressReporter{ProgressReport}, System.Threading.CancellationToken)"/>
    Task PreUpdateGameAsync(ProgressReporter<ProgressReport>? reporter = null, CancellationToken token = default);

    /// <summary>
    /// Updates the game to the latest version. This method downloads the game's update package
    /// (unless it is already downloaded e.g. through <see cref="PreUpdateGameAsync(Dodoco.Core.ProgressReporter{ProgressReport}, System.Threading.CancellationToken)"/>),
    /// unzips it, applies its patches and then remove deprecated files from the old version.
    /// </summary>
    Task UpdateGameAsync(IGameIntegrityManager integrityManager, ProgressReporter<ProgressReport>? reporter = null, CancellationToken token = default);

}