using Dodoco.Core.Network.Api.Github.Repos.Release;
using Dodoco.Core.Network.HTTP;

namespace Dodoco.Core.Wine {

    public interface IWinePackageManager: IStatefulEntity<WinePackageManagerState> {

        string PackagesRootDirectory { get; }

        event EventHandler AfterReleaseDownload;

        Task InstallPackageFromRelease(Release release, ProgressReporter<ProgressReport>? progress = null);
        IWine GetWineFromTag(string releaseTagName, string prefixDirectory);
        List<string> GetInstalledTags();
        Task<List<Release>> GetAvaliableReleases();
        Task<Release> GetLatestRelease();

    }

}