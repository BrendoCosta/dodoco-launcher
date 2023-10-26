namespace Dodoco.Core.Game;

public interface IGameVoiceManager {

    /// <summary>
    /// Returns a list with all installed voices packages' languages for current game installation.
    /// To achieve this, the method simply verify if each supported language from <see cref="F:Dodoco.Core.Game.GameLanguage.All"/> does have a 
    /// folder named after it inside "*_Data/StreamingAssets/AudioAssets" directory.
    /// </summary>
    /// <returns>
    /// A list containing all installed voices packages' languages for current game installation.
    /// </returns>
    IEnumerable<GameLanguage> GetInstalledVoices();

}