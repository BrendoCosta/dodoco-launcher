using System.Text;

namespace Dodoco.Core.Game {

    public struct GameApi {

        public string Url { get; set; }
        public string Key { get; set; }
        public int LauncherId { get; set; }

        public static GameApi GetDefault(GameServer server) {

            if (server == GameServer.Global) {

                return new GameApi {

                    Url = Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9oazRlLWxhdW5jaGVyLXN0YXRpYy5ob3lvdmVyc2UuY29tL2hrNGVfZ2xvYmFsL21kay9sYXVuY2hlci9hcGk=")),
                    Key = Encoding.UTF8.GetString(Convert.FromBase64String("Z2NTdGdhcmg=")),
                    LauncherId = 10

                };

            } else {

                return new GameApi {

                    Url = Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9zZGstc3RhdGljLm1paG95by5jb20vaGs0ZV9jbi9tZGsvbGF1bmNoZXIvYXBp")),
                    Key = Encoding.UTF8.GetString(Convert.FromBase64String("ZVlkODlKbUo=")),
                    LauncherId = 18

                };

            }

        }

    }

}