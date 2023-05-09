using Dodoco.Application;
using Dodoco.Game;
using Dodoco.Util.Log;

using System.Globalization;

namespace Dodoco.Launcher {

    public record LauncherSettings: ApplicationFile<LauncherSettings> {

        public LauncherSettings(): base(
            "settings",
            ApplicationConstants.APPLICATION_HOME_DIRECTORY,
            LauncherConstants.LAUNCHER_SETTINGS_FILENAME
        ) {}

        public Launcher launcher = new Launcher();
        public Wine wine = new Wine();
        public Game game = new Game();
        public Api api = new Api();

        // Schema

        public record Launcher {

            public CultureInfo language = new CultureInfo("en-US");
            public bool display_splash_screen = true;

        }

        public record Wine {

            public string prefix_path = Path.Join(ApplicationConstants.APPLICATION_HOME_DIRECTORY, "wine");

        }

        public record Game {

            public CultureInfo language = new CultureInfo("en-US");
            public GameServer server = GameServer.global;
            public string installation_path = Path.Join(ApplicationConstants.APPLICATION_HOME_DIRECTORY, "game");
            public List<CultureInfo> voices = new List<CultureInfo>{
                GameConstants.DEFAULT_VOICE_LANGUAGE
            };

        }

        public record Api {

            public Dictionary<GameServer, CompanyApi> company = new Dictionary<GameServer, CompanyApi> {

                { GameServer.global, new CompanyApi() },
                { GameServer.chinese, new CompanyApi {

                    url = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9zZGstc3RhdGljLm1paG95by5jb20vaGs0ZV9jbi9tZGsvbGF1bmNoZXIvYXBp")),
                    key = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("ZVlkODlKbUo=")),
                    launcher_id = 18

                }},

            };

            public record CompanyApi {

                public string url = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9oazRlLWxhdW5jaGVyLXN0YXRpYy5ob3lvdmVyc2UuY29tL2hrNGVfZ2xvYmFsL21kay9sYXVuY2hlci9hcGk="));
                public string key = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("Z2NTdGdhcmg="));
                public int launcher_id = 10;

            }
            
        }

    }

}