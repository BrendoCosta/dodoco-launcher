using System.Text;

namespace Dodoco.Game {

    public static class GameConstants {

        public static Dictionary<GameServer, string> GAME_TITLE { get; private set; } = new Dictionary<GameServer, string> {
            { GameServer.global, Encoding.UTF8.GetString(Convert.FromBase64String("R2Vuc2hpbkltcGFjdA==")) },
            { GameServer.chinese, Encoding.UTF8.GetString(Convert.FromBase64String("WXVhblNoZW4=")) }
        };

    }

}