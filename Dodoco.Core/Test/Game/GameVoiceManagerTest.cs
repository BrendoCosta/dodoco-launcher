namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using NUnit.Framework;

[TestFixture]
public class GameVoiceManagerTest {

    private static List<GameLanguage> GetInstalledVoices_Cases = GameLanguage.All;

    [TestCaseSource(nameof(GetInstalledVoices_Cases))]
    public void GetInstalledVoices_Test(GameLanguage language) {

        GameSettings settings = new GameSettings();
        settings.InstallationDirectory = Path.Join(Util.TEST_STATIC_DIRECTOY_PATH, "/Game/GameVoiceManagerTest/");
        IGameEx game = new GameEx(settings);
        IGameVoiceManager voiceManager = new GameVoiceManager(game);
        Assert.IsTrue(voiceManager.GetInstalledVoices().Contains(language));

    }

}