using Dodoco.Application;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Network.HTTP;

namespace Dodoco.Game {

    public interface IGame {

        GameState State { get; }
        
        event EventHandler<ApplicationProgressReport> OnCheckIntegrityProgress;
        event EventHandler<DownloadProgressReport> OnDownloadProgress;

        Task<List<GameIntegrityReport>> CheckIntegrity(CancellationToken token = default);
        Task Download(CancellationToken token = default);
        
        void SetInstallationDirectory(string directory);
        void SetVersion(Version version);

        Version GetVersion();

    }

}