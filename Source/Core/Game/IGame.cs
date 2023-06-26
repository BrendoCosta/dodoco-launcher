using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Wine;

namespace Dodoco.Core.Game {

    public interface IGame {

        string InstallationDirectory { get; }
        GameState State { get; }
        Version Version { get; }
        IWine Wine { get; }
        bool IsUpdating { get; }

        void SetInstallationDirectory(string directory);
        void SetVersion(Version version);

        string GetInstallationDirectory();
        GameState GetGameState();
        Version GetVersion();

        Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(CancellationToken token = default);
        Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(ProgressReporter<ProgressReport> progress, CancellationToken token = default);
        Task Download(CancellationToken token = default);
        Task RepairFile(GameFileIntegrityReport report, CancellationToken token = default);
        Task RepairFile(GameFileIntegrityReport report, ProgressReporter<ProgressReport>? progress, CancellationToken token = default);
        Task RepairGameFiles(ProgressReporter<ProgressReport>? progress, CancellationToken token = default);
        Task RepairGameFiles(CancellationToken token = default);
        Task Update(ProgressReporter<ProgressReport>? progress = null);

    }

}