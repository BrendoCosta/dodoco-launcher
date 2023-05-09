using Dodoco.Application;

namespace Dodoco.Launcher {

    public static class LauncherConstants {

        // Directories

        public static string LAUNCHER_CACHE_DIRECTORY { get; private set; } = Path.Join(ApplicationConstants.APPLICATION_HOME_DIRECTORY, $".cache");
        
        // Filenames
        
        public static string LAUNCHER_SETTINGS_FILENAME { get; private set; } = "settings.yaml";
        public static string LAUNCHER_CACHE_FILENAME { get; private set; } = "cache.yaml";

    }

}