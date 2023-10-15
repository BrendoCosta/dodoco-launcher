namespace Dodoco.Core.Game;

using Dodoco.Core.Util.Log;

/// <summary>
/// Class <c>GameVoiceManager</c> contains methods to manage game's voices packages.
/// </summary>
public class GameVoiceManager: IGameVoiceManager {

    private IGameEx Game;
    
    public GameVoiceManager(IGameEx game) => this.Game = game;

    /// <summary>
    /// Returns a list with all installed voices packages' languages for current game installation.
    /// To achieve this, the method simply verify if each supported language from <c>GameLanguage.All</c> does have a 
    /// folder named after it inside "*_Data/StreamingAssets/AudioAssets" directory.
    /// </summary>
    /// <returns>
    /// A list containing all installed voices packages' languages for current game installation.
    /// </returns>
    public virtual List<GameLanguage> GetInstalledVoices() {

        Logger.GetInstance().Log($"Checking installed game voice packages...");

        List<GameLanguage> installedVoices = new List<GameLanguage>();

        foreach (GameLanguage language in GameLanguage.All) {

            if (Directory.Exists(Path.Join(this.Game.Settings.InstallationDirectory, this.Game.GetDataDirectoryName(), "/StreamingAssets/AudioAssets/", language.Name))) {

                Logger.GetInstance().Log($"The voice package \"{language.Name}\" ({language.Code}) is installed");
                installedVoices.Add(language);

            }

        }

        Logger.GetInstance().Log($"Successfully checked installed game voice packages");

        return installedVoices;

    }

}