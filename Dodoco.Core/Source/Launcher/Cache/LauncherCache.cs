using Dodoco.Core.Network;
using Dodoco.Core.Network.Api.Company.Launcher.Content;
using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Util.Hash;
using Dodoco.Core.Util.Log;
using Dodoco.Core.Util.FileSystem;

using System.Text.RegularExpressions;

namespace Dodoco.Core.Launcher.Cache {

    public record LauncherCache {

        public Resource Resource { get; set; } = new Resource();
        public Content Content { get; set; } = new Content();
        public BackgroundImage background_image { get; set; } = new BackgroundImage();

        // Schema

        public record BackgroundImage {

            public string filename { get; set; } = "background.png";
            public string md5_hash { get; set; } = "";

        }

        // Methods

        public async Task UpdateFromContent(Content content) {

            Logger.GetInstance().Log($"Updating launcher's cache...");

            

            Logger.GetInstance().Log($"Successfully updated launcher's cache");

        }

    }

}