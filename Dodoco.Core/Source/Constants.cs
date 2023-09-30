namespace Dodoco.Core {

    public static class Constants {

        public static string IDENTIFIER { get; private set; } = "dodoco-launcher";
        
        // Directories
        
        public static string HOME_DIRECTORY { get; private set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"/.local/share/{IDENTIFIER}");
        public static string LOG_DIRECTORY { get; private set; } = Path.Join(HOME_DIRECTORY, $"log");

        // Filenames

        public static string LOG_FILENAME { get; private set; } = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.log";

    }

}