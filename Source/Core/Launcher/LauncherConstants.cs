using Dodoco.Core;

namespace Dodoco.Core.Launcher {

    public static class LauncherConstants {

        // Directories

        public static string CACHE_DIRECTORY { get; private set; } = Path.Join(Constants.HOME_DIRECTORY, $".cache");
        public static string SETTINGS_DIRECTORY { get; private set; } = Constants.HOME_DIRECTORY;
        public static string BACKGROUND_IMAGE_DIRECTORY { get; private set; } = CACHE_DIRECTORY;
        
        // Filenames
        
        public static string SETTINGS_FILENAME { get; private set; } = "settings.yaml";
        public static string CACHE_FILENAME { get; private set; } = "cache.json";
        public static string BACKGROUND_IMAGE_FILENAME { get; private set; } = "background_image.png";

    }

}