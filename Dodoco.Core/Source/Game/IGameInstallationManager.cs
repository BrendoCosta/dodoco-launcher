namespace Dodoco.Core.Game;

public interface IGameInstallationManager: IStatefulEntity<GameInstallationManagerState> {

    /// <returns>
    /// The sum of the size of each game's segment to be downloaded.
    /// </returns>
    Task<long> GetGamePackageDownloadSize();
    
    /// <summary>
    /// Downloads and unzips all game's segments to the game installation folder.
    /// </summary>
    Task InstallGameAsync(ProgressReporter<ProgressReport>? progress, bool forceReinstall = false, CancellationToken token = default);

}