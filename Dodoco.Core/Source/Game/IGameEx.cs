namespace Dodoco.Core.Game;

using Dodoco.Core.Network.Api.Company;

public interface IGameEx {

    GameSettings Settings { get; set; }
    CompanyApiFactory ApiFactory { get; }

    bool CheckGameInstallation();
    string GetDataDirectoryName();
    string GetGameTitle();
    string GetMainExecutableName();
    Task<Version> GetGameVersionAsync();

}