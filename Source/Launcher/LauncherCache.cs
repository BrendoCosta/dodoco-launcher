using Dodoco.Util.Log;

namespace Dodoco.Launcher {

    public record LauncherCache: LauncherFile {

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

    }

}