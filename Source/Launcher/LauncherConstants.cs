namespace Dodoco.Launcher {

    public static class LauncherConstants {

        public static string LAUNCHER_HOME_DIRECTORY { get; private set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $"/.local/share/{Dodoco.Application.ApplicationConstants.DEFAULT_APPLICATION_IDENTIFIER}/");
        public static string LAUNCHER_SETTINGS_FILENAME { get; private set; } = "settings.yaml";

    }

}