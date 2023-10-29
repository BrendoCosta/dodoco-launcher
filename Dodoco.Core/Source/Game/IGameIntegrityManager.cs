namespace Dodoco.Core.Game;

public interface IGameIntegrityManager: IStatefulEntity<GameIntegrityManagerState> {

    /// <inheritdoc cref="GetInstallationIntegrityReportAsync(Dodoco.Core.ProgressReporter{ProgressReport}?, System.Threading.CancellationToken)"/>
    Task<List<GameFileIntegrityReportEx>> GetInstallationIntegrityReportAsync();
    
    /// <summary>
    /// Verify all game's files searching for either missing files or files whose checksum diffears
    /// from the one in pkg_version.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="System.Collections.Generic.List{T}(Dodoco.Core.Game.GameFileIntegrityReportEx)"/>
    /// referencing the missing files or files whose checksum diffears
    /// from the one in pkg_version.
    /// </returns>
    Task<List<GameFileIntegrityReportEx>> GetInstallationIntegrityReportAsync(ProgressReporter<ProgressReport>? reporter, CancellationToken token = default);
    
    /// <inheritdoc cref="RepairInstallationAsync(System.Collections.Generic.List{Dodoco.Core.Game.GameFileIntegrityReportEx}, Dodoco.Core.ProgressReporter{ProgressReport}?, System.Threading.CancellationToken)"/>
    Task<List<GameFileIntegrityReportEx>> RepairInstallationAsync(List<GameFileIntegrityReportEx> reports, CancellationToken token = default);
    
    /// <summary>
    /// Attempts to repair all referenced game files from the list provided by downloading
    /// them from the remote server.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="System.Collections.Generic.List{T}(Dodoco.Core.Game.GameFileIntegrityReportEx)"/>
    /// that references successfully repaired input files.
    /// </returns>
    Task<List<GameFileIntegrityReportEx>> RepairInstallationAsync(List<GameFileIntegrityReportEx> reports, ProgressReporter<ProgressReport>? reporter, CancellationToken token = default);

    /// <summary>
    /// Downloads and parses the pkg_version for the current game's version.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="System.Collections.Generic.List{T}(Dodoco.Core.Game.GamePkgVersionEntry)"/>
    /// with all entries parsed from the pkg_version file.
    /// </returns>
    Task<List<GamePkgVersionEntry>> GetPkgVersionAsync();

}