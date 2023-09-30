
using Dodoco.Core.Game;
using Dodoco.Core.Network.Api.Github.Repos;

using System.Globalization;

namespace Dodoco.Core.Launcher.Settings {

    public record LauncherSettings {

        public LauncherConfig Launcher { get; set; } = new LauncherConfig();
        public WineConfig Wine { get; set; } = new WineConfig();
        public GameSettings Game { get; set; } = new GameSettings();
        public ApiConfig Api { get; set; } = new ApiConfig();

        // Schema

        public record LauncherConfig {

            public bool AutoSearchForUpdates { get; set; } = true;
            public CultureInfo Language { get; set; } = new CultureInfo("en-US");

        }

        public record WineConfig {

            public bool UserDefinedInstallation { get; set; } = false;
            public string? InstallationDirectory { get; set; }
            public string SelectedRelease { get; set; } = "GE-Proton8-12";
            public string ReleasesDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "wine");
            public string PrefixDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "wine", "prefix");

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
            
        }

    }

}