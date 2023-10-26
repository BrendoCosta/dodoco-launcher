namespace Dodoco.Core.Protocol.Company.Launcher;

public abstract class LauncherResponse {

    public LauncherResponseStatus retcode { get; set; } = LauncherResponseStatus.ERROR;
    public string? message { get; set; }

    public bool IsSuccessfull() {

        return this.retcode == LauncherResponseStatus.SUCCESS;

    }

    /// <summary>
    /// Assert the object contains a successful response code from the server.
    /// Throws a <see cref="T:Dodoco.Core.Protocol.ProtocolException" /> if the
    /// response code received from the server is not a <see cref="F:Dodoco.Core.Protocol.Company.Launcher.LauncherResponseStatus.SUCCESS" />.
    /// </summary>
    /// <exception cref="T:Dodoco.Core.Protocol.ProtocolException">
    ///     The response code received from the server is not a <see cref="F:Dodoco.Core.Protocol.Company.Launcher.LauncherResponseStatus.SUCCESS" />.
    /// </exception>
    public void EnsureSuccessStatusCode() {

        if (!this.IsSuccessfull())
            throw new ProtocolException("Received an unsuccessful response code from the server");

    }

}