using Dodoco.Core.Network.Api.Github.Repos.Release;
using Dodoco.Core.Network.HTTP;

namespace Dodoco.Core.Wine {

    public interface IWinePackageManager {

        string PackagesRootDirectory { get; }

        Task InstallPackageFromRelease(Release release, ProgressReporter<DownloadProgressReport> progress);
        IWine GetWineFromTag(string releaseTagName, string prefixDirectory);
        List<string> GetInstalledTags();
        Task<List<Release>> GetAvaliableReleases();
        Task<Release> GetLatestRelease();

    }

}