namespace Dodoco.Core.Game {

    public interface IMutableGame: IGame {

        Task RepairFile(GameFileIntegrityReport report, CancellationToken token = default);
        Task RepairGameFiles(ProgressReporter<ProgressReport>? progress, CancellationToken token = default);
        Task RepairGameFiles(CancellationToken token = default);
        Task Download(CancellationToken token = default);
        
        void SetInstallationDirectory(string directory);
        void SetVersion(Version version);

    }

}