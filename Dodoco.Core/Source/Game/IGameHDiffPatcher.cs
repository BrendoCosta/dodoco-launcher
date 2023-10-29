namespace Dodoco.Core.Game;

public interface IGameHDiffPatcher {

    Task Patch(string diffPath, string oldFilePath, string newFilePath);

}