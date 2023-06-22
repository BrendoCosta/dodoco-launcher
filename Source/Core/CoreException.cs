namespace Dodoco.Core {

    public class CoreException: Exception {

        public CoreException(): base() {}
        public CoreException(string? message): base(message) {}
        public CoreException(string? message, Exception? innerException): base(message, innerException) {}

    }

}