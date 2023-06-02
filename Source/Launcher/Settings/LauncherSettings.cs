
using Dodoco.Application;
using Dodoco.Game;

using System.Globalization;

namespace Dodoco.Launcher.Settings {

    public record LauncherSettings {

        public Launcher launcher { get; set; } = new Launcher();
        public Wine wine { get; set; } = new Wine();
        public Game game { get; set; } = new Game();
        public Api api { get; set; } = new Api();

        // Schema

        public record Launcher {

            public CultureInfo language { get; set; } = new CultureInfo("en-US");
            public bool display_splash_screen { get; set; } = true;

        }

        public record Wine {

            public string prefix_path { get; set; } = Path.Join(ApplicationConstants.APPLICATION_HOME_DIRECTORY, "wine");

        }

        public record Game {

            public CultureInfo language { get; set; } = new CultureInfo("en-US");
            public GameServer server { get; set; } = GameServer.global;
            public string installation_path { get; set; } = Path.Join(ApplicationConstants.APPLICATION_HOME_DIRECTORY, "game");
            public List<CultureInfo> voices { get; set; } = new List<CultureInfo>{
                GameConstants.DEFAULT_VOICE_LANGUAGE
            };

        }

        public record Api {

            public GithubApi dodoco { get; set; } = new GithubApi { url = new Uri("https://api.github.com/repos/BrendoCosta/dodoco-launcher/") };

            public Dictionary<GameServer, CompanyApi> company { get; set; } = new Dictionary<GameServer, CompanyApi> {

                { GameServer.global, new CompanyApi() },
                { GameServer.chinese, new CompanyApi {

                    url = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9zZGstc3RhdGljLm1paG95by5jb20vaGs0ZV9jbi9tZGsvbGF1bmNoZXIvYXBp")),
                    key = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("ZVlkODlKbUo=")),
                    launcher_id = 18

                }},

            };

            public record CompanyApi {

                public string url { get; set; } = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9oazRlLWxhdW5jaGVyLXN0YXRpYy5ob3lvdmVyc2UuY29tL2hrNGVfZ2xvYmFsL21kay9sYXVuY2hlci9hcGk="));
                public string key { get; set; } = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("Z2NTdGdhcmg="));
                public int launcher_id { get; set; } = 10;

            }

            public record GithubApi {

                public Uri url { get; set; } = new Uri("https://api.github.com");

            }
            
        }

    }

}