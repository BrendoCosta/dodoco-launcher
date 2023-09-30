namespace Dodoco.Core.Launcher {

    public class LauncherException: CoreException {

        public LauncherException(): base() {}
        public LauncherException(string? message): base(message) {}
        public LauncherException(string? message, Exception? innerException): base(message, innerException) {}

    }

}