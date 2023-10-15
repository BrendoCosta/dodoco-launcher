namespace Dodoco.Core.Game;

public interface IGameEx {

    GameSettings Settings { get; set; }

    string GetDataDirectoryName();
    string GetGameTitle();
    string GetMainExecutableName();
    

}