using System.Globalization;
using System.Text;

namespace Dodoco.Core.Game {

    public static class GameConstants {

        public static Dictionary<GameServer, string> GAME_TITLE { get; private set; } = new Dictionary<GameServer, string> {
            { GameServer.Global, Encoding.UTF8.GetString(Convert.FromBase64String("R2Vuc2hpbkltcGFjdA==")) },
            { GameServer.Chinese, Encoding.UTF8.GetString(Convert.FromBase64String("WXVhblNoZW4=")) }
        };

        public static CultureInfo DEFAULT_VOICE_LANGUAGE = new CultureInfo("en-US");
        public static List<CultureInfo> SUPPORTED_VOICE_LANGUAGES = new List<CultureInfo> {
            new CultureInfo("en-US"),
            new CultureInfo("zh-CN"),
            new CultureInfo("ko-KR"),
            new CultureInfo("ja-JP")
        };

    }

}