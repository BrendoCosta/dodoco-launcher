using Dodoco.Core.Wine;

namespace Dodoco.Core.Game {

    public interface IGame {

        string InstallationDirectory { get; }
        GameState State { get; }
        Version Version { get; }
        IWine Wine { get; }

        string GetInstallationDirectory();
        GameState GetGameState();
        Version GetVersion();

        Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(CancellationToken token = default);
        Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(ProgressReporter<ProgressReport> progress, CancellationToken token = default);

    }

}