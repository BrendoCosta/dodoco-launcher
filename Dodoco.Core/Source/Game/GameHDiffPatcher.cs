namespace Dodoco.Core.Game;

using Hi3Helper.SharpHDiffPatch;

public class GameHDiffPatcher: IGameHDiffPatcher {

    public async Task Patch(string diffPath, string oldFilePath, string newFilePath) {

        Task patchTask = new Task(() => {

            HDiffPatch sharpPatcher = new HDiffPatch();
            sharpPatcher.Initialize(diffPath);
            sharpPatcher.Patch(oldFilePath, newFilePath, true);
            
        });

        patchTask.Start();
        await patchTask;

    }

}