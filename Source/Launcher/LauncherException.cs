namespace Dodoco.Launcher {

    public class LauncherException: Dodoco.Application.ApplicationException {

        public LauncherException(): base() {}
        public LauncherException(string? message): base(message) {}
        public LauncherException(string? message, Exception? innerException): base(message, innerException) {}

    }

}