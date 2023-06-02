namespace Dodoco.Serialization {

    public class SerializationException: Dodoco.Application.ApplicationException {

        public SerializationException(): base() {}
        public SerializationException(string? message): base(message) {}
        public SerializationException(string? message, Exception? innerException): base(message, innerException) {}

    }

}