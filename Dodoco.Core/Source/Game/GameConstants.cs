using System.Globalization;
using System.Text;

namespace Dodoco.Core.Game {

    public static class GameConstants {

        public static Dictionary<GameServer, string> GAME_TITLE { get; private set; } = new Dictionary<GameServer, string> {
            { GameServer.Global, Encoding.UTF8.GetString(Convert.FromBase64String("R2Vuc2hpbkltcGFjdA==")) },
            { GameServer.Chinese, Encoding.UTF8.GetString(Convert.FromBase64String("WXVhblNoZW4=")) }
        };

    }

}