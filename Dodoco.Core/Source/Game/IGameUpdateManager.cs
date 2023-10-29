namespace Dodoco.Core.Game;

public interface IGameUpdateManager: IStatefulEntity<GameUpdateManagerState> {

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