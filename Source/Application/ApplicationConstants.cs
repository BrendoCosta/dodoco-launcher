namespace Dodoco.Application {

    public static class ApplicationConstants {

        public static string DEFAULT_APPLICATION_IDENTIFIER { get; private set; } = "dodoco-launcher";
        public static string APPLICATION_BUNDLE_FOLDER_NAME { get; private set; } = "Bundle";
        public static int DEFAULT_APPLICATION_TCP_PORT { get; private set; } = 4000;
        
        // Directories
        
        public static string APPLICATION_HOME_DIRECTORY { get; private set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"/.local/share/{DEFAULT_APPLICATION_IDENTIFIER}/");
        public static string APPLICATION_LOG_DIRECTORY { get; private set; } = Path.Join(APPLICATION_HOME_DIRECTORY, $"log");

        // Filenames

        public static string APPLICATION_LOG_FILENAME { get; private set; } = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.log";

    }

}