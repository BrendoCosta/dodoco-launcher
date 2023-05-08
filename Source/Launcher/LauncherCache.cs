using Dodoco.Api.Company.Launcher;
using Dodoco.Util.Log;

using System.Text.RegularExpressions;

namespace Dodoco.Launcher {

    public record LauncherCache: LauncherFile<LauncherCache> {

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

        public async Task UpdateFromContentApi(Content content) {

            if (!string.IsNullOrWhiteSpace(content.data.adv.background)) {

                string remoteBackgroundMD5Checksum = "";

                foreach (Match match in Regex.Matches(content.data.adv.background, @"([a-zA-Z0-9]+)_([0-9]+).png")) {

                    remoteBackgroundMD5Checksum = match.ToString().Split("_")[0];

                }

                if (this.background_image.md5_hash != remoteBackgroundMD5Checksum) {

                    HttpResponseMessage response = await Application.Application.GetInstance().client.FetchAsync(content.data.adv.background);
                    
                    if (response.IsSuccessStatusCode) {

                        // Save file

                    } else {

                        Logger.GetInstance().Error($"Unable to download background image (received HTTP status code {response.StatusCode})");

                    }

                }

            }

        }

    }

}