using Dodoco.Core;
using Dodoco.Core.Network.Api.Github.Repos;
using Dodoco.Core.Network.Api.Github.Repos.Release;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Util.FileSystem;
using Dodoco.Core.Util.Hash;
using Dodoco.Core.Util.Log;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Dodoco.Core.Wine {

    public class WineGeCustomPackageManager: IWinePackageManager {

        public WinePackageManagerState State { get; private set; } = WinePackageManagerState.READY;
        public string PackagesRootDirectory { get; private set; }

        private const string RELEASE_TAG_NAME_PATTERN = @"GE-Proton[\d]+-[\d]+";
        private const string RELEASE_DIRECTORY_PATTERN = @"lutris-GE-Proton[\d]+-[\d]+-x86_64";
        private const string RELEASE_FILENAME_PATTERN = @"wine-lutris-GE-Proton[\d]+-[\d]+-x86_64.tar.xz";
        private const string RELEASE_CHECKSUM_FILENAME_PATTERN = @"wine-lutris-GE-Proton[\d]+-[\d]+-x86_64.sha512sum";
        
        private GitHubReposApiFactory apiFactory;
        private Cache<List<Release>> avaliableReleasesCache = new Cache<List<Release>>(new List<Release>());

        public event EventHandler<WinePackageManagerState> OnStateUpdate = delegate {};
        public event EventHandler AfterReleaseDownload = delegate {};

        public WineGeCustomPackageManager(string packagesRootDirectory, GitHubReposApiFactory apiFactory) {

            this.PackagesRootDirectory = packagesRootDirectory;
            this.apiFactory = apiFactory;

        }

        public async Task InstallPackageFromRelease(Release release, ProgressReporter<ProgressReport>? progress = null) {

            WinePackageManagerState previousState = this.State;
            this.UpdateState(WinePackageManagerState.DOWNLOADING_PACKAGE);

            try {

                Logger.GetInstance().Log($"Installing the release \"{release.tag_name}\"...");

                Asset tarballAsset = release.assets.Find(x => Regex.IsMatch(x.name, WineGeCustomPackageManager.RELEASE_FILENAME_PATTERN));
                Asset checksumAsset = release.assets.Find(x => Regex.IsMatch(x.name, WineGeCustomPackageManager.RELEASE_CHECKSUM_FILENAME_PATTERN));

                if (!release.assets.Contains(tarballAsset))
                    throw new WineException($"Unable to find the tarball asset for the requested release \"{release.tag_name}\"");
                
                if (!release.assets.Contains(checksumAsset))
                    throw new WineException($"Unable to find the checksum asset for the requested release \"{release.tag_name}\"");
                
                long storageFreeBytes = FileSystem.GetAvaliableStorageSpace(this.PackagesRootDirectory);
                if (storageFreeBytes <= (tarballAsset.size + checksumAsset.size))
                    throw new WineException("There is not enough space available in the storage device for Wine's installation");
                
                Logger.GetInstance().Log($"Downloading the release file \"{Path.Join(this.PackagesRootDirectory, tarballAsset.name)}\"...");

                await Client.GetInstance().DownloadFileAsync(new Uri(tarballAsset.browser_download_url), Path.Join(this.PackagesRootDirectory, tarballAsset.name), progress);
                await Client.GetInstance().DownloadFileAsync(new Uri(checksumAsset.browser_download_url), Path.Join(this.PackagesRootDirectory, checksumAsset.name), progress);

                this.UpdateState(WinePackageManagerState.CHECKING_PACKAGE_CHECKSUM);
                Logger.GetInstance().Log($"Computing release file's SHA-512 checksum...");

                string tarballChecksum = new Hash(SHA512.Create()).ComputeHash(Path.Join(this.PackagesRootDirectory, tarballAsset.name));
                string expectedChecksum = (await File.OpenText(Path.Join(this.PackagesRootDirectory, checksumAsset.name)).ReadToEndAsync()).Substring(0, 128).ToUpper();

                if (tarballChecksum == expectedChecksum) {

                    Logger.GetInstance().Log($"The downloaded tarball file's SHA-512 checksum match the expected checksum ({expectedChecksum})");

                } else {

                    throw new WineException($"The downloaded tarball file's SHA-512 checksum ({tarballChecksum}) doesn't match the expected checksum ({expectedChecksum})");

                }

                this.UpdateState(WinePackageManagerState.DECOMPRESSING_PACKAGE);
                Logger.GetInstance().Log($"Decompressing the release file \"{Path.Join(this.PackagesRootDirectory, tarballAsset.name)}\"...");

                Process process = new Process();
                
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "/usr/bin/tar";
                info.WorkingDirectory = this.PackagesRootDirectory;
                info.Arguments = $"-xvJf {Path.Join(this.PackagesRootDirectory, tarballAsset.name)}";
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = false;

                process.StartInfo = info;
                
                process.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) => {
                    
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        Logger.GetInstance().Log($"TAR: {e.Data}");
                    
                });

                process.ErrorDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) => {
                    
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        throw new WineException($"TAR: {e.Data}");
                    
                });

                try {

                    if (process.Start()) {

                        Logger.GetInstance().Log($"Successfully started TAR's process inside directory \"{info.WorkingDirectory}\" with the command \"{info.FileName} {info.Arguments}\"");

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        await process.WaitForExitAsync();

                        Logger.GetInstance().Log($"Successfully finished TAR's process");

                    } else {

                        throw new WineException($"Failed to start the TAR's process inside directory \"{info.WorkingDirectory}\" with the command \"{info.FileName} {info.Arguments}\"");

                    }

                } catch (Exception e) {

                    throw new WineException($"Failed to start the TAR's process inside directory \"{info.WorkingDirectory}\" with the command \"{info.FileName} {info.Arguments}\"", e);

                }

                Logger.GetInstance().Log($"Successfully installed the release \"{release.tag_name}\"");
                this.AfterReleaseDownload.Invoke(this, EventArgs.Empty);

            } finally {

                this.UpdateState(previousState);

            }

            return;

        }

        public IWine GetWineFromTag(string releaseTagName, string prefixDirectory) {

            return new Wine(Path.Join(this.PackagesRootDirectory, $"lutris-{releaseTagName}-x86_64"), prefixDirectory);

        }

        public List<string> GetInstalledTags() {

            List<string> installedReleases = new List<string>();

            foreach (string subdirectory in Directory.GetDirectories(this.PackagesRootDirectory)) {

                string subdirectoryName = new DirectoryInfo(subdirectory).Name;

                foreach (Match match in Regex.Matches(subdirectoryName, WineGeCustomPackageManager.RELEASE_TAG_NAME_PATTERN)) {

                    if (File.Exists(Path.Join(subdirectory, "/bin/", "wine"))) {

                        installedReleases.Add(match.Value);

                    }

                }

            }

            return installedReleases;

        }

        public async Task<List<Release>> GetAvaliableReleases() {

            Logger.GetInstance().Log($"Fetching avaliable Wine's releases...");

            if (this.avaliableReleasesCache.IsValid()) {

                Logger.GetInstance().Log($"Returned previously fetched avaliable Wine's releases from the cache");
                return avaliableReleasesCache.Resource;

            } else {

                List<Release> avaliableReleases = new List<Release>();
                List<Release>? fetchedReleases = await this.apiFactory.FetchReleases();
                
                if (fetchedReleases != null) {

                    foreach (Release release in fetchedReleases) {

                        if (Regex.IsMatch(release.tag_name, WineGeCustomPackageManager.RELEASE_TAG_NAME_PATTERN))
                            avaliableReleases.Add(release);

                    }

                }

                this.avaliableReleasesCache.Update(avaliableReleases, TimeSpan.FromMinutes(5));
                Logger.GetInstance().Log($"Updated the cache with the fetched avaliable Wine's releases");
                
                Logger.GetInstance().Log($"Successfully fetched avaliable Wine's releases");
                return avaliableReleases;

            }

        }

        public async Task<Release> GetLatestRelease() {

            return (await this.GetAvaliableReleases()).First();

        }

        private void UpdateState(WinePackageManagerState newState) {

            Logger.GetInstance().Debug($"Updating package manager's state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;
            this.OnStateUpdate.Invoke(this, newState);

        }

    }

}