namespace Dodoco.Core.Protocol.Company.Launcher;

public abstract class LauncherResponse {

    public LauncherResponseStatus retcode { get; set; } = LauncherResponseStatus.ERROR;
    public string? message { get; set; }

    public bool IsSuccessfull() {

        return this.retcode == LauncherResponseStatus.SUCCESS;

    }

}