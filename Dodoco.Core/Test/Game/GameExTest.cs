namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using NUnit.Framework;

[TestFixture]
public class GameExTest {

    private GameSettings Settings;
    private IGameEx Game;

    [SetUp]
    public void Init() {

        this.Settings = new GameSettings();
        this.Game = new GameEx(this.Settings);

    }

    [Test]
    public void GetDataDirectoryName_Global_Test() {

        this.Game.Settings.Server = GameServer.Global;
        Assert.AreEqual(Game.GetDataDirectoryName(), "GenshinImpact_Data");

    }

    [Test]
    public void GetDataDirectoryName_Chinese_Test() {

        this.Game.Settings.Server = GameServer.Chinese;
        Assert.AreEqual(Game.GetDataDirectoryName(), "YuanShen_Data");

    }

    [Test]
    public void GetGameTitle_Global_Test() {

        this.Game.Settings.Server = GameServer.Global;
        Assert.AreEqual(Game.GetGameTitle(), "GenshinImpact");

    }

    [Test]
    public void GetGameTitle_Chinese_Test() {

        this.Game.Settings.Server = GameServer.Chinese;
        Assert.AreEqual(Game.GetGameTitle(), "YuanShen");

    }

    [Test]
    public void GetMainExecutableName_Global_Test() {

        this.Game.Settings.Server = GameServer.Global;
        Assert.AreEqual(Game.GetMainExecutableName(), "GenshinImpact.exe");

    }

    [Test]
    public void GetMainExecutableName_Chinese_Test() {

        this.Game.Settings.Server = GameServer.Chinese;
        Assert.AreEqual(Game.GetMainExecutableName(), "YuanShen.exe");

    }

}