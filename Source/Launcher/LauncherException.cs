namespace Dodoco.Launcher {

    public class LauncherException: Exception {

        public LauncherException(): base() {}
        public LauncherException(string? message): base(message) {}
        public LauncherException(string? message, Exception? innerException): base(message, innerException) {}

    }

}