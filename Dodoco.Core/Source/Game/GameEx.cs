namespace Dodoco.Core.Game;

using System.Text;

public class GameEx: IGameEx {

    public GameSettings Settings { get; set; }

    public GameEx(GameSettings settings) {

        this.Settings = settings;

    }

    public string GetDataDirectoryName() {

        return this.GetGameTitle() + "_Data";

    }

    public string GetGameTitle() {

       return this.Settings.Server == GameServer.Global ? "GenshinImpact" : "YuanShen";

    }

    public string GetMainExecutableName() {

        return this.GetGameTitle() + ".exe";

    }

}