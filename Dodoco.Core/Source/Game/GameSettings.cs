using System.Globalization;

namespace Dodoco.Core.Game {

    public record GameSettings {

        public CultureInfo Language { get; set; } = new CultureInfo("en-US");
        public GameServer Server { get; set; } = GameServer.Global;
        public string InstallationDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "game");
        public List<GameLanguage> Voices { get; set; } = new List<GameLanguage>{
            GameLanguage.Default
        };
        public Dictionary<GameServer, GameApi> Api { get; set; } = new Dictionary<GameServer, GameApi> {

            { GameServer.Global, GameApi.GetDefault(GameServer.Global) },
            { GameServer.Chinese, GameApi.GetDefault(GameServer.Chinese) },

        };

    }

}