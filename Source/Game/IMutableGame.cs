using Dodoco.Application;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Network.HTTP;

namespace Dodoco.Game {

    public interface IMutableGame: IGame {

        Task RepairFile(GameFileIntegrityReport report, CancellationToken token = default);
        Task Download(CancellationToken token = default);
        
        void SetInstallationDirectory(string directory);
        void SetVersion(Version version);

    }

}