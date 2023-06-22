namespace Dodoco.Core.Network {

    public class NetworkException: CoreException {

        public NetworkException(): base() {}
        public NetworkException(string? message): base(message) {}
        public NetworkException(string? message, Exception? innerException): base(message, innerException) {}

    }

}