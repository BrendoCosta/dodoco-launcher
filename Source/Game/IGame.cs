using Dodoco.Application;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Network.HTTP;

namespace Dodoco.Game {

    public interface IGame {

        event EventHandler<ApplicationProgressReport> OnCheckIntegrityProgress;
        event EventHandler<DownloadProgressReport> OnDownloadProgress;

        string InstallationDirectory { get; }
        GameState State { get; }
        Version Version { get; }

        string GetInstallationDirectory();
        GameState GetGameState();
        Version GetVersion();

        Task<List<GameFileIntegrityReport>> CheckFilesIntegrity(CancellationToken token = default);

    }

}