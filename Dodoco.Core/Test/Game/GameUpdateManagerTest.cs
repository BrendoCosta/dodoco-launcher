namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Protocol.Company.Launcher;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Serialization;
using Dodoco.Core.Serialization.Json;

using Moq;
using NUnit.Framework;

[TestFixture]
public class GameUpdateManagerTest {

    private GameSettings Settings;
    private IGameEx Game;

    private static object[] GetGameUpdateAsync_Test_Cases = {
        new object[] { GameServer.Chinese },
        new object[] { GameServer.Global }
    };

    [SetUp]
    public void Init() {

        this.Settings = new GameSettings();
        this.Game = new GameEx(this.Settings);

    }

    [TestCaseSource(nameof(GetGameUpdateAsync_Test_Cases))]
    [Description("GetGameUpdateAsync() should return null when the game is not installed")]
    public async Task GetGameUpdateAsync_Game_Not_Installed(GameServer server) {

        this.Game.Settings.Server = server;
        this.Game.Settings.InstallationDirectory = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, Guid.NewGuid().ToString());

        IGameUpdateManager updateManager = new GameUpdateManager(this.Game);
        Assert.IsNull(await updateManager.GetGameUpdateAsync());

    }

    [TestCaseSource(nameof(GetGameUpdateAsync_Test_Cases))]
    [Description("GetGameUpdateAsync() should return null if the game is updated")]
    public async Task GetGameUpdateAsync_Game_Update(GameServer server) {

        GameSettings settings = new GameSettings {

            Server = server

        };

        Mock<GameEx> gameMock = new Mock<GameEx>(settings);
        gameMock.CallBase = true;
        // Lets assume the game is installed
        gameMock.Setup(m => m.CheckGameInstallation()).Returns(true);
        // Lets assume the game is updated
        const string UPDATED_VERSION = "4.0.1";
        ResourceResponse res = new ResourceResponse {
            retcode = LauncherResponseStatus.SUCCESS,
            data = new ResourceData {
                game = new ResourceGame {
                    latest = new ResourceLatest {
                        version = UPDATED_VERSION
                    }
                }
            }
        };
        
        Mock<CompanyApiFactory> apiFactoryMock = new Mock<CompanyApiFactory>(
            this.Settings.Api[this.Settings.Server].Url,
            this.Settings.Api[this.Settings.Server].Key,
            this.Settings.Api[this.Settings.Server].LauncherId,
            this.Settings.Language
        );
        apiFactoryMock.CallBase = true;
        apiFactoryMock.Setup(m => m.FetchLauncherResource()).Returns(Task<ResourceResponse>.FromResult(res));

        gameMock.Setup(m => m.GetGameVersionAsync()).Returns(Task<ResourceResponse>.FromResult(Version.Parse(UPDATED_VERSION)));
        gameMock.Setup(m => m.GetApiFactory()).Returns(apiFactoryMock.Object);

        IGameUpdateManager updateManager = new GameUpdateManager(gameMock.Object);
        Assert.That(await updateManager.GetGameUpdateAsync(), Is.Null);

    }

    [TestCaseSource(nameof(GetGameUpdateAsync_Test_Cases))]
    [Description("GetGameUpdateAsync() should return the update's resource if the game is outdated")]
    public async Task GetGameUpdateAsync_Game_Outdated(GameServer server) {

        GameSettings settings = new GameSettings {

            Server = server

        };

        Mock<GameEx> gameMock = new Mock<GameEx>(settings);
        gameMock.CallBase = true;
        // Lets assume the game is installed
        gameMock.Setup(m => m.CheckGameInstallation()).Returns(true);
        // Lets assume there is an update and the game is outdated
        ResourceResponse res = new ResourceResponse {
            retcode = LauncherResponseStatus.SUCCESS,
            data = new ResourceData {
                game = new ResourceGame {
                    latest = new ResourceLatest {
                        version = "4.0.1"
                    }
                }
            }
        };
        
        Mock<CompanyApiFactory> apiFactoryMock = new Mock<CompanyApiFactory>(
            this.Settings.Api[this.Settings.Server].Url,
            this.Settings.Api[this.Settings.Server].Key,
            this.Settings.Api[this.Settings.Server].LauncherId,
            this.Settings.Language
        );
        apiFactoryMock.CallBase = true;
        apiFactoryMock.Setup(m => m.FetchLauncherResource()).Returns(Task<ResourceResponse>.FromResult(res));

        gameMock.Setup(m => m.GetGameVersionAsync()).Returns(Task<ResourceResponse>.FromResult(Version.Parse("3.8.0")));
        gameMock.Setup(m => m.GetApiFactory()).Returns(apiFactoryMock.Object);

        IGameUpdateManager updateManager = new GameUpdateManager(gameMock.Object);
        IFormatSerializer serializer = new JsonSerializer();
        Assert.That(serializer.Serialize(await updateManager.GetGameUpdateAsync()), Is.EqualTo(serializer.Serialize(res.data.game)));

    }

}