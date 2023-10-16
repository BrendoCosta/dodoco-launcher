using Dodoco.Core.Network;
using Dodoco.Core.Protocol.Company.Launcher.Content;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Util.Hash;
using Dodoco.Core.Util.Log;

using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Dodoco.Core.Launcher {

    public class LauncherBackgroundImageFile: WritableManagedFile<byte[]> {

        public string Hash { get; private set; } = string.Empty;

        public LauncherBackgroundImageFile(): base(
            "background image",
            LauncherConstants.BACKGROUND_IMAGE_DIRECTORY,
            LauncherConstants.BACKGROUND_IMAGE_FILENAME
        ) {

            this.UpdateHash();

        }

        public void UpdateHash() {

            if (this.Exist()) {

                this.Hash = new Hash(MD5.Create()).ComputeHash(this.FullPath);

            }

        }

        public override byte[] Read() {

            if (!this.Exist())
                throw new LauncherException($"The launcher's background image's file doesn't exist");

            return File.ReadAllBytes(this.FullPath);

        }

        public override void Write(byte[] content) {

            throw new NotImplementedException();
            
        }

        public async Task Update(ContentResponse content) => await this.Update(content, null);
        public async Task Update(ContentResponse content, ProgressReporter<ProgressReport>? progress) {

            if (!string.IsNullOrWhiteSpace(content.data.adv.background.ToString())) {

                string remoteBackgroundMD5Checksum = "";

                foreach (Match match in Regex.Matches(content.data.adv.background.ToString(), @"([a-zA-Z0-9]+)_([0-9]+).([a-zA-Z0-9]{3,4})")) {

                    remoteBackgroundMD5Checksum = match.ToString().Split("_")[0];

                }

                if (this.Hash.ToUpper() != remoteBackgroundMD5Checksum.ToUpper()) {

                    Logger.GetInstance().Warning($"Remote launcher's background image MD5 hash is \"{remoteBackgroundMD5Checksum.ToUpper()}\" while cached is \"{this.Hash.ToUpper()}\"");
                    Logger.GetInstance().Log($"Downloading launcher's background image...");

                    try {

                        await Client.GetInstance().DownloadFileAsync(
                            content.data.adv.background,
                            Path.Join(this.Directory, this.FileName),
                            progress
                        );

                        this.UpdateHash();
                        Logger.GetInstance().Log($"Updated launcher's background image's MD5 hash to {this.Hash}");

                    } catch (NetworkException e) {

                        Logger.GetInstance().Error($"Failed to download launcher's background image", e);

                    }

                    Logger.GetInstance().Log($"Successfully downloaded launcher's background image");

                }

            } else {

                Logger.GetInstance().Error($"Received empty launcher's background image URL");

            }

        }

    }

}