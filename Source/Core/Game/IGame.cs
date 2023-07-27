using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Wine;

namespace Dodoco.Core.Game {

    public interface IGame: IStatefulEntity<GameState> {

        GameSettings Settings { get; set; }
        IWine? Wine { get; set; }
        Version Version { get; }
        bool IsInstalled { get; }

        event EventHandler AfterGameDownload;
        event EventHandler AfterGameUpdate;

        Task<bool> IsUpdateAvaliable();
        Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(CancellationToken token = default);
        Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(ProgressReporter<ProgressReport> progress, CancellationToken token = default);
        Task Download(ProgressReporter<ProgressReport>? progress, CancellationToken token = default);
        Task RepairFile(GameFileIntegrityReport report, CancellationToken token = default);
        Task RepairFile(GameFileIntegrityReport report, ProgressReporter<ProgressReport>? progress, CancellationToken token = default);
        Task RepairGameFiles(ProgressReporter<ProgressReport>? progress, CancellationToken token = default);
        Task RepairGameFiles(CancellationToken token = default);
        Task Update(ProgressReporter<ProgressReport>? progress = null);
        Task Start();

    }

}