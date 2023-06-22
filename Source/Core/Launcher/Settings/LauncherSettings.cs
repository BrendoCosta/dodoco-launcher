
using Dodoco.Core.Game;
using Dodoco.Core.Network.Api.Github.Repos;

using System.Globalization;

namespace Dodoco.Core.Launcher.Settings {

    public record LauncherSettings {

        public LauncherConfig Launcher { get; set; } = new LauncherConfig();
        public WineConfig Wine { get; set; } = new WineConfig();
        public GameConfig Game { get; set; } = new GameConfig();
        public ApiConfig Api { get; set; } = new ApiConfig();

        // Schema

        public record LauncherConfig {

            public bool AutoSearchForUpdates { get; set; } = true;
            public CultureInfo Language { get; set; } = new CultureInfo("en-US");

        }

        public record WineConfig {

            public bool UserDefinedInstallation { get; set; } = false;
            public string? InstallationDirectory { get; set; }
            public string SelectedRelease { get; set; } = "GE-Proton8-8";
            public string ReleasesDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "wine");
            public string PrefixDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "wine", "prefix");

        }

        public record GameConfig {

            public CultureInfo Language { get; set; } = new CultureInfo("en-US");
            public GameServer Server { get; set; } = GameServer.Global;
            public string InstallationDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "game");
            public List<CultureInfo> Voices { get; set; } = new List<CultureInfo>{
                GameConstants.DEFAULT_VOICE_LANGUAGE
            };

        }

        public record ApiConfig {

            public GitHubReposApiConfig Launcher { get; set; } = new GitHubReposApiConfig {

                Owner = "BrendoCosta",
                Repository = "dodoco-launcher"

            };

            public GitHubReposApiConfig Wine { get; set; } = new GitHubReposApiConfig {

                Owner = "GloriousEggroll",
                Repository = "wine-ge-custom"
                
            };

            public Dictionary<GameServer, CompanyApi> Company { get; set; } = new Dictionary<GameServer, CompanyApi> {

                { GameServer.Global, new CompanyApi() },
                { GameServer.Chinese, new CompanyApi {

                    Url = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9zZGstc3RhdGljLm1paG95by5jb20vaGs0ZV9jbi9tZGsvbGF1bmNoZXIvYXBp")),
                    Key = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("ZVlkODlKbUo=")),
                    LauncherId = 18

                }},

            };

            public record CompanyApi {

                public string Url { get; set; } = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9oazRlLWxhdW5jaGVyLXN0YXRpYy5ob3lvdmVyc2UuY29tL2hrNGVfZ2xvYmFsL21kay9sYXVuY2hlci9hcGk="));
                public string Key { get; set; } = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("Z2NTdGdhcmg="));
                public int LauncherId { get; set; } = 10;

            }
            
        }

    }

}