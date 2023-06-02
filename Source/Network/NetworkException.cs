namespace Dodoco.Network {

    public class NetworkException: Dodoco.Application.ApplicationException {

        public NetworkException(): base() {}
        public NetworkException(string? message): base(message) {}
        public NetworkException(string? message, Exception? innerException): base(message, innerException) {}

    }

}