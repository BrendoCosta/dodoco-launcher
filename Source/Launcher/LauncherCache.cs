using Dodoco.Application;
using Dodoco.Api.Company.Launcher;
using Dodoco.Util.Hash;
using Dodoco.Util.Log;

using System.Text.RegularExpressions;

namespace Dodoco.Launcher {

    public record LauncherCache: ApplicationFile<LauncherCache> {

        public LauncherCache(): base(
            "cache",
            LauncherConstants.LAUNCHER_CACHE_DIRECTORY,
            LauncherConstants.LAUNCHER_CACHE_FILENAME
        ) {}

        public BackgroundImage background_image = new BackgroundImage();

        public record BackgroundImage {

            public string filename = "background.png";
            public string md5_hash = "";

        }

        public async Task UpdateFromContent(Content content) {

            Logger.GetInstance().Log($"Updating launcher's cache...");
            bool cacheChanged = false;

            if (!string.IsNullOrWhiteSpace(content.data.adv.background)) {

                string remoteBackgroundMD5Checksum = "";

                foreach (Match match in Regex.Matches(content.data.adv.background, @"([a-zA-Z0-9]+)_([0-9]+).([a-zA-Z0-9]{3,4})")) {

                    remoteBackgroundMD5Checksum = match.ToString().Split("_")[0];

                }

                if (this.background_image.md5_hash.ToUpper() != remoteBackgroundMD5Checksum.ToUpper()) {

                    cacheChanged = true;

                    Logger.GetInstance().Log($"Downloading launcher's background image...");

                    HttpResponseMessage response = await Application.Application.GetInstance().client.FetchAsync(content.data.adv.background);
                    
                    if (response.IsSuccessStatusCode) {

                        Logger.GetInstance().Log($"Successfully downloaded launcher's background image");

                        byte[] fileContent = await response.Content.ReadAsByteArrayAsync();

                        if (fileContent.Length > 0) {

                            Logger.GetInstance().Log($"Writing launcher's background image to storage...");
                            string fileName = $"background.{content.data.adv.background.Split(".").Last()}";
                            File.WriteAllBytes(Path.Join(LauncherConstants.LAUNCHER_CACHE_DIRECTORY, fileName), fileContent);
                            Logger.GetInstance().Log($"Successfully wrote launcher's background image to storage");
                            
                            string newHash = MD5.ComputeHash(fileContent);
                            this.background_image.md5_hash = newHash;
                            Logger.GetInstance().Log($"Updated launcher's background image's MD5 hash to {newHash}");

                        } else {

                            Logger.GetInstance().Error($"The downloaded launcher's background image file is empty");

                        }

                    } else {

                        Logger.GetInstance().Error($"Failed to download background image (received HTTP status code {response.StatusCode})");

                    }

                }

            }

            if (cacheChanged) {

                Logger.GetInstance().Log($"Launcher's cache has been changed and will be updated");
                this.WriteFile();

            }

            Logger.GetInstance().Log($"Successfully updated launcher's cache");

        }

    }

}