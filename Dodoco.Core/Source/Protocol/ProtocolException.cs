namespace Dodoco.Core.Protocol;

public class ProtocolException: CoreException {

    public ProtocolException(): base() {}
    public ProtocolException(string? message): base(message) {}
    public ProtocolException(string? message, Exception? innerException): base(message, innerException) {}

}