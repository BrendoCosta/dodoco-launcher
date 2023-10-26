namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using NUnit.Framework;

[TestFixture]
public class GameExTest {

    private GameSettings Settings;
    private IGameEx Game;

    private static object[] CheckGameInstallation_Cases = {
        new object[] { GameServer.Chinese, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/CheckGameInstallation_Test"), true },
        new object[] { GameServer.Global, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/CheckGameInstallation_Test"), true },
        new object[] { GameServer.Chinese, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, Guid.NewGuid().ToString()), false },
        new object[] { GameServer.Global, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, Guid.NewGuid().ToString()), false }
    };

    private static object[] GetDataDirectoryName_Cases = {
        new object[] { GameServer.Chinese, "YuanShen_Data" },
        new object[] { GameServer.Global, "GenshinImpact_Data" }
    };

    private static object[] GetGameTitle_Cases = {
        new object[] { GameServer.Chinese, "YuanShen" },
        new object[] { GameServer.Global, "GenshinImpact" }
    };

    private static object[] GetMainExecutableName_Cases = {
        new object[] { GameServer.Chinese, "YuanShen.exe" },
        new object[] { GameServer.Global, "GenshinImpact.exe" }
    };

    private static object[] GetGameVersionAsync_GlobalGameManagers_Cases = {
        new object[] { GameServer.Chinese, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/GetGameVersionAsync_GlobalGameManagers_Test"), Version.Parse("4.1.0") },
        new object[] { GameServer.Global, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/GetGameVersionAsync_GlobalGameManagers_Test"), Version.Parse("4.1.0") }
    };

    private static object[] GetGameVersionAsync_UnityPlayer_Cases = {
        new object[] { GameServer.Chinese, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/GetGameVersionAsync_UnityPlayer_Test"), Version.Parse("4.1.0") },
        new object[] { GameServer.Global, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/GetGameVersionAsync_UnityPlayer_Test"), Version.Parse("4.1.0") }
    };

    private static object[] GetGameVersionAsync_Remote_Cases = {
        new object[] { GameServer.Chinese, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, Guid.NewGuid().ToString()) },
        new object[] { GameServer.Global, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, Guid.NewGuid().ToString()) }
    };

    [SetUp]
    public void Init() {

        this.Settings = new GameSettings();
        this.Game = new GameEx(this.Settings);

    }

    [TestCaseSource(nameof(CheckGameInstallation_Cases))]
    public void CheckGameInstallation_Test(GameServer server, string directory, bool expected) {

        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = directory;
        Assert.That(this.Game.CheckGameInstallation(), Is.EqualTo(expected));

    }

    [TestCaseSource(nameof(GetDataDirectoryName_Cases))]
    public void GetDataDirectoryName_Test(GameServer server, string expected) {

        this.Game.Settings.Server = server;
        Assert.That(Game.GetDataDirectoryName(), Is.EqualTo(expected));

    }

    [TestCaseSource(nameof(GetGameTitle_Cases))]
    public void GetGameTitle_Test(GameServer server, string expected) {

        this.Game.Settings.Server = server;
        Assert.That(Game.GetGameTitle(), Is.EqualTo(expected));

    }

    [TestCaseSource(nameof(GetMainExecutableName_Cases))]
    public void GetMainExecutableName_Test(GameServer server, string expected) {

        this.Game.Settings.Server = server;
        Assert.That(Game.GetMainExecutableName(), Is.EqualTo(expected));

    }

    [TestCaseSource(nameof(GetGameVersionAsync_GlobalGameManagers_Cases))]
    public virtual async Task GetGameVersionAsync_GlobalGameManagers_Test(GameServer server, string directory, Version expected) {

        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = directory;
        Assert.That(await this.Game.GetGameVersionAsync(), Is.EqualTo(expected));

    }

    [TestCaseSource(nameof(GetGameVersionAsync_UnityPlayer_Cases))]
    public virtual async Task GetGameVersionAsync_UnityPlayer_Test(GameServer server, string directory, Version expected) {

        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = directory;
        Assert.That(await this.Game.GetGameVersionAsync(), Is.EqualTo(expected));

    }

    [TestCaseSource(nameof(GetGameVersionAsync_Remote_Cases))]
    public virtual async Task GetGameVersionAsync_Remote_Test(GameServer server, string directory) {

        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = directory;
        Version expectedRemoteVersion = Version.Parse((await this.Game.ApiFactory.FetchLauncherResource()).data.game.latest.version);
        Version remoteVersionTaken = await this.Game.GetGameVersionAsync();
        Assert.That(remoteVersionTaken, Is.EqualTo(expectedRemoteVersion));

    }

}