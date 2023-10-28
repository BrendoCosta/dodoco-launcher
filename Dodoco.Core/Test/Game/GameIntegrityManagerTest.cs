namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using Dodoco.Core.Util.FileSystem;
using NUnit.Framework;
using Moq;

[TestFixture]
public class GameIntegrityManagerTest {

    /*
     * We will be mocking GetPkgVersionAsync() because we really
     * don't want to download the entire game LMAO.
    */

    private static string pkg_version = string.Join("\r\n", new List<string> {
        "{\"remoteName\": \"GenshinImpact_Data/Native/Data/etc/mono/2.0/settings.map\", \"md5\": \"4574da87289d555309a47f34757d6cf1\", \"fileSize\": 2670}",
        "{\"remoteName\": \"GenshinImpact_Data/Native/Data/etc/mono/4.0/Browsers/Compat.browser\", \"md5\": \"3201df8753c86b4be9cc69c046883d3c\", \"fileSize\": 1627}",
        "{\"remoteName\": \"GenshinImpact_Data/Managed/Resources/Newtonsoft.Json.dll-resources.dat\", \"md5\": \"ef68c753a3826e16a982d610340a3484\", \"fileSize\": 639}"
    });

    private GameSettings settings = new GameSettings {

        Server = GameServer.Global,
        InstallationDirectory = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameIntegrityManagerTest/GetInstallationIntegrityReportAsync_Test")

    };

    [Test]
    public async Task GameIntegrityManager_GetInstallationIntegrityReportAsync_Test() {

        IGameEx game = new GameEx(settings);
        Mock<GameIntegrityManager> mock = new Mock<GameIntegrityManager>(game);
        mock.CallBase = true; // Methods who are not present in the setup should fallback to base implementation
        mock.Setup(m => m.GetPkgVersionAsync()).Returns(
            Task.FromResult(PkgVersionParser.Parse(pkg_version))
        );

        List<GameFileIntegrityReportEx> filesToFix = await mock.Object.GetInstallationIntegrityReportAsync();

        Assert.IsTrue(filesToFix.Exists(someFile =>
            someFile.State == GameFileIntegrityState.MISSING
            && someFile.Path == "GenshinImpact_Data/Native/Data/etc/mono/2.0/settings.map"
            && someFile.LocalChecksum == string.Empty
            && someFile.LocalSize == 0
            && someFile.RemoteChecksum == "4574da87289d555309a47f34757d6cf1".ToUpper()
            && someFile.RemoteSize == 2670L
        ));

        Assert.IsTrue(filesToFix.Exists(someFile =>
            someFile.State == GameFileIntegrityState.MISSING
            && someFile.Path == "GenshinImpact_Data/Native/Data/etc/mono/4.0/Browsers/Compat.browser"
            && someFile.LocalChecksum == string.Empty
            && someFile.LocalSize == 0
            && someFile.RemoteChecksum == "3201df8753c86b4be9cc69c046883d3c".ToUpper()
            && someFile.RemoteSize == 1627L
        ));

        Assert.IsTrue(filesToFix.Exists(someFile =>
            someFile.State == GameFileIntegrityState.CORRUPTED
            && someFile.Path == "GenshinImpact_Data/Managed/Resources/Newtonsoft.Json.dll-resources.dat"
            && someFile.LocalChecksum == "0ad540d7681bcd2bf0d49bff7811d596".ToUpper() // md5sum /Test.Static/Game/GameIntegrityManagerTest/GetInstallationIntegrityReportAsync_Test/GenshinImpact_Data/Managed/Resources/Newtonsoft.Json.dll-resources.dat
            && someFile.LocalSize == 57L
            && someFile.RemoteChecksum == "ef68c753a3826e16a982d610340a3484".ToUpper()
            && someFile.RemoteSize == 639L
        ));

    }

    [Test]
    public async Task GameIntegrityManager_RepairInstallationAsync_Test() {

        /*
         * Copies the test directory use by GameIntegrityManager_GetInstallationIntegrityReportAsync_Test()
        */

        const string SOURCE_DIRECTORY = "/Game/GameIntegrityManagerTest/GetInstallationIntegrityReportAsync_Test";
        const string TARGET_DIRECTORY = "/Game/GameIntegrityManagerTest/.RepairInstallationAsync_Test";
        
        FileSystem.CopyDirectory(Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, SOURCE_DIRECTORY), Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, TARGET_DIRECTORY), true);
        
        IGameEx game = new GameEx(settings);
        Mock<GameIntegrityManager> mock = new Mock<GameIntegrityManager>(game);
        mock.CallBase = true; // Methods who are not present in the setup should fallback to base implementation
        mock.Setup(m => m.GetPkgVersionAsync()).Returns(
            Task.FromResult(PkgVersionParser.Parse(pkg_version))
        );

        List<GameFileIntegrityReportEx> filesToFix = await mock.Object.GetInstallationIntegrityReportAsync();
        List<GameFileIntegrityReportEx> fixedFiles = await mock.Object.RepairInstallationAsync(filesToFix);
        
        // If we check the installation again, there should be no missing/corrupted files
        
        Assert.That((await mock.Object.GetInstallationIntegrityReportAsync()).Count, Is.EqualTo(0));

    }

}