using Dodoco.Application;
using Dodoco.Network;
using Dodoco.Network.Api.Company.Launcher.Content;
using Dodoco.Network.HTTP;
using Dodoco.Util.Hash;
using Dodoco.Util.Log;
using Dodoco.Util.Unit;

using System.Text.RegularExpressions;

namespace Dodoco.Launcher.Cache {

    public record LauncherCache {

        public BackgroundImage background_image { get; set; } = new BackgroundImage();

        // Schema

        public record BackgroundImage {

            public string filename { get; set; } = "background.png";
            public string md5_hash { get; set; } = "";

        }

        // Methods

        public async Task UpdateFromContent(Content content) {

            Logger.GetInstance().Log($"Updating launcher's cache...");

            if (!string.IsNullOrWhiteSpace(content.data.adv.background.ToString())) {

                string remoteBackgroundMD5Checksum = "";

                foreach (Match match in Regex.Matches(content.data.adv.background.ToString(), @"([a-zA-Z0-9]+)_([0-9]+).([a-zA-Z0-9]{3,4})")) {

                    remoteBackgroundMD5Checksum = match.ToString().Split("_")[0];

                }

                if (this.background_image.md5_hash.ToUpper() != remoteBackgroundMD5Checksum.ToUpper()) {

                    Logger.GetInstance().Warning($"Remote launcher's background image MD5 hash is \"{remoteBackgroundMD5Checksum.ToUpper()}\" while cached is \"{this.background_image.md5_hash.ToUpper()}\"");
                    Logger.GetInstance().Log($"Downloading launcher's background image...");
                    
                    ApplicationProgress<DownloadProgressReport> prg = new ApplicationProgress<DownloadProgressReport>();
                    prg.ProgressChanged += new EventHandler<DownloadProgressReport>((object? sender, DownloadProgressReport e) => {

                        Logger.GetInstance().Debug($"Progress: {e.completionPercentage}%");
                        Logger.GetInstance().Debug($"Speed: {DataUnitFormatter.Format(e.bytesPerSecond, DataUnitFormatterOption.USE_SYMBOL)}/s");
                        Logger.GetInstance().Debug($"ETA: {e.estimatedRemainingTime.ToString(@"hh\:mm\:ss")}");

                    });

                    try {

                        await Application.Application.GetInstance().client.DownloadFileAsync(
                            content.data.adv.background,
                            Path.Join(LauncherConstants.LAUNCHER_CACHE_DIRECTORY, this.background_image.filename),
                            prg
                        );

                        string newHash = MD5.ComputeHash(new FileInfo(Path.Join(LauncherConstants.LAUNCHER_CACHE_DIRECTORY, this.background_image.filename)));
                        this.background_image.md5_hash = newHash;
                        Logger.GetInstance().Log($"Updated launcher's background image's MD5 hash to {newHash}");

                    } catch (NetworkException e) {

                        Logger.GetInstance().Error($"Failed to download background image", e);

                    }

                    Logger.GetInstance().Log($"Successfully downloaded launcher's background image");

                }

            }

            Logger.GetInstance().Log($"Successfully updated launcher's cache");

        }

    }

}