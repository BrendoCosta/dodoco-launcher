namespace Dodoco.Core.Serialization {

    public class SerializationException: CoreException {

        public SerializationException(): base() {}
        public SerializationException(string? message): base(message) {}
        public SerializationException(string? message, Exception? innerException): base(message, innerException) {}

    }

}