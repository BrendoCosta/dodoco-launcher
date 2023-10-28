namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Embed;
using Dodoco.Core.Game;
using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Serialization;
using Dodoco.Core.Serialization.Json;
using Moq;
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

    private static object[] GetResource_Updated_Test_Cases = {
        new object[] { GameServer.Chinese },
        new object[] { GameServer.Global }
    };

    private static object[] GetResource_Outdated_Test_Cases = {
        new object[] { GameServer.Chinese, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/GetResource_Outdated_Test") },
        new object[] { GameServer.Global, Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/GetResource_Outdated_Test") }
    };

    private static object[] UpdateGameResourceCache_Test_Cases = {
        new object[] { GameServer.Chinese },
        new object[] { GameServer.Global }
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

    [TestCaseSource(nameof(GetResource_Updated_Test_Cases))]
    public async Task GetResource_Updated_Test(GameServer server) {

        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, Guid.NewGuid().ToString());
        ResourceResponse expected = await this.Game.ApiFactory.FetchLauncherResource();
        ResourceResponse taken = await this.Game.GetResourceAsync();
        IFormatSerializer serializer = new JsonSerializer();
        Assert.That(serializer.Serialize(taken), Is.EqualTo(serializer.Serialize(expected)));

    }

    [TestCaseSource(nameof(GetResource_Outdated_Test_Cases))]
    public async Task GetResource_Outdated_Test(GameServer server, string directory) {

        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = directory;
        ResourceResponse expected = EmbeddedResourceManager.GetLauncherResource(server, Version.Parse("3.8.0"));
        ResourceResponse taken = await this.Game.GetResourceAsync();
        IFormatSerializer serializer = new JsonSerializer();
        Assert.That(serializer.Serialize(taken), Is.EqualTo(serializer.Serialize(expected)));

    }

    [TestCaseSource(nameof(UpdateGameResourceCache_Test_Cases))]
    public async Task UpdateGameResourceCache_Test(GameServer server) {

        Version targetTestVersion = Version.Parse("3.8.0");
        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameExTest/UpdateGameResourceCache_Test");
        
        GameResourceCacheFile resourceCacheFile = new GameResourceCacheFile();
        Predicate<GameResourceCache> desiredCacheEntry = new Predicate<GameResourceCache>(e =>
            e.Server == server
            && Version.Parse(e.Resource.data.game.latest.version) == targetTestVersion
        );

        // If the cache file does exist, it shouldn't contain the cache entry

        if (resourceCacheFile.Exist()) {

            Assert.IsFalse(resourceCacheFile.Read().Exists(desiredCacheEntry));

        }

        await this.Game.UpdateGameResourceCacheAsync();

        // Now both the file and the cache entry should exist

        Assert.IsTrue(resourceCacheFile.Read().Exists(desiredCacheEntry));

        // Remove the created entry from the list

        var list = resourceCacheFile.Read();
        list.RemoveAll(desiredCacheEntry);
        resourceCacheFile.Write(list);

    }

}