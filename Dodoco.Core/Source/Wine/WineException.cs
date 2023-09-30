namespace Dodoco.Core.Wine {

    public class WineException: CoreException {

        public WineException(): base() {}
        public WineException(string? message): base(message) {}
        public WineException(string? message, Exception? innerException): base(message, innerException) {}

    }

}