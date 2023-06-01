using Dodoco.Application;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Network.HTTP;

namespace Dodoco.Game {

    public interface IGame {

        GameState State { get; }

        ApplicationProgressReport CheckIntegrityProgressReport { get; }
        DownloadProgressReport DownloadProgressReport { get; }

        Task<List<GameIntegrityReport>> CheckIntegrity(CancellationToken token = default);
        Task<List<GameIntegrityReport>> CheckIntegrity(ApplicationProgress<ApplicationProgressReport> progress, CancellationToken token = default);
        Task Download(CancellationToken token = default);
        Task Download(ApplicationProgress<DownloadProgressReport> progress, CancellationToken token = default);

        void SetInstallationDirectory(string directory);
        void SetVersion(Version version);

        Version GetVersion();

    }

}