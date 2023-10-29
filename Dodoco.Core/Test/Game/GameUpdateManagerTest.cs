namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using Dodoco.Core.Network.Api.Company;
using Dodoco.Core.Protocol.Company.Launcher;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Serialization;
using Dodoco.Core.Serialization.Json;
using Dodoco.Core.Util.FileSystem;
using Dodoco.Core.Util.Hash;

using Moq;
using NUnit.Framework;
using System.Security.Cryptography;

[TestFixture]
public class GameUpdateManagerTest {

    private GameSettings Settings;
    private IGameEx Game;

    private static object[] GetGameUpdateAsync_Test_Cases = {
        new object[] { GameServer.Chinese },
        new object[] { GameServer.Global }
    };

    private static object[] UpdateGameAsync_Test_Cases = {
        new object[] { "UpdateGameAsync_Test" },
        new object[] { "UpdateGameAsync_Without_HDiffFiles_or_DeleteFiles_Test" }
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

    [TestCaseSource(nameof(UpdateGameAsync_Test_Cases)), Description("Test updating an outdated game version to a new version")]
    public async Task UpdateGameAsync_Test(string textDirectory) {

        // Copy the test files into a test directory

        string testRootDirectory = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameUpdateManagerTest/", textDirectory);
        string sourceDirectory = Path.Join(testRootDirectory, "/game_4.0.1/");
        string targetDirectory = Path.Join(testRootDirectory, "/.UpdateGameAsync_Test/");
        const string testPackageFilename = "game_4.0.1_4.1.0_hdiff_QSwRBvbj1gaAs7zG.zip";
        string testPackagePath = Path.Join(testRootDirectory, testPackageFilename);

        FileSystem.CopyDirectory(sourceDirectory, targetDirectory, true);
        File.Copy(testPackagePath, Path.Join(targetDirectory, testPackageFilename), true);

        // The game installation will be set to the test directory

        this.Game.Settings.Server = GameServer.Global;
        this.Game.Settings.InstallationDirectory = targetDirectory;

        Mock<GameIntegrityManager> integrityManagerMock = new Mock<GameIntegrityManager>(this.Game);
        integrityManagerMock.CallBase = true;
        integrityManagerMock.Setup(m => m.GetPkgVersionAsync()).Returns(Task.FromResult(
            PkgVersionParser.Parse(
                string.Join("\r\n", File.ReadAllLines(
                    Path.Join(testRootDirectory, "/game_4.0.1/pkg_version")
                    )
                )
            )
        ));

        Assert.That(
            (await integrityManagerMock.Object.GetInstallationIntegrityReportAsync()).Count,
            Is.EqualTo(0),
            "The old version game installation must be upright in order to this test run"
        );

        // Mocks GetGameUpdateAsync() to return a 4.1.0 ResourceGame

        Mock<GameUpdateManager> updateManagerMock = new Mock<GameUpdateManager>(this.Game);
        updateManagerMock.CallBase = true;
        updateManagerMock.Setup(m => m.GetGameUpdateAsync()).Returns(Task.FromResult(
            (ResourceGame?) new ResourceGame {
                latest = new ResourceLatest { version = "4.1.0" },
                diffs = new List<ResourceDiff> {
                    new ResourceDiff {
                        name = testPackageFilename,
                        path = string.Empty,
                        size = new FileInfo(testPackagePath).Length,
                        md5 = new Hash(MD5.Create()).ComputeHash(testPackagePath)
                    }
                }
            }
        ));

        await updateManagerMock.Object.UpdateGameAsync(integrityManagerMock.Object);

        // The game should be at version 4.1.0 now

        Assert.That(await this.Game.GetGameVersionAsync(), Is.EqualTo(Version.Parse("4.1.0")));

        // Returns the fake local pkg_version instead that from ther server

        var helperGetNewVersionPkgVersion = () => {
            return PkgVersionParser.Parse(
                string.Join("\r\n", File.ReadAllLines(
                    Path.Join(testRootDirectory, "/game_4.1.0/pkg_version")
                    )
                )
            );
        };

        foreach (GamePkgVersionEntry entry in helperGetNewVersionPkgVersion()) {

            Assert.That(
                File.Exists(Path.Join(this.Settings.InstallationDirectory, entry.remoteName)),
                Is.True,
                "All files from the new version should exist"
            );

        }
        
        GameDeleteFiles gameDeleteFiles = new GameDeleteFiles(this.Settings.InstallationDirectory);

        if (gameDeleteFiles.Exist()) {

            foreach (var fileToDeletePath in gameDeleteFiles.Read()) {

                Assert.That(
                    File.Exists(Path.Join(this.Settings.InstallationDirectory, fileToDeletePath)),
                    Is.False,
                    "All deprecated files should have been removed from the new version"
                );

            }

        }

        Mock<GameIntegrityManager> anotherIntegrityManagerMock = new Mock<GameIntegrityManager>(this.Game);
        anotherIntegrityManagerMock.CallBase = true;
        anotherIntegrityManagerMock.Setup(m => m.GetPkgVersionAsync()).Returns(Task.FromResult(helperGetNewVersionPkgVersion()));
        
        Assert.That(
            (await anotherIntegrityManagerMock.Object.GetInstallationIntegrityReportAsync()).Count,
            Is.EqualTo(0),
            "The updated game should contain no errors i.e. all hdiff patches should have been successfully applied"
        );

    }

}