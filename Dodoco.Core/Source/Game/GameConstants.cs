using System.Globalization;
using System.Text;

namespace Dodoco.Core.Game {

    public static class GameConstants {

        public static Dictionary<GameServer, string> GAME_TITLE { get; private set; } = new Dictionary<GameServer, string> {
            { GameServer.Global, Encoding.UTF8.GetString(Convert.FromBase64String("R2Vuc2hpbkltcGFjdA==")) },
            { GameServer.Chinese, Encoding.UTF8.GetString(Convert.FromBase64String("WXVhblNoZW4=")) }
        };

        public static GameLanguage DEFAULT_VOICE_LANGUAGE {
            
            get => new GameLanguage { Code = "en-us", Name = "English(US)" };
            
        }

        public static List<GameLanguage> SUPPORTED_VOICE_LANGUAGES {
            
            get => new List<GameLanguage> {

                GameConstants.DEFAULT_VOICE_LANGUAGE,
                new GameLanguage { Code = "zh-cn", Name = "Chinese" },
                new GameLanguage { Code = "ja-jp", Name = "Japanese" },
                new GameLanguage { Code = "ko-kr", Name = "Korean" }

            };

        }

    }

}