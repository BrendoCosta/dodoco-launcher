namespace Dodoco.Launcher {

    public record LauncherSettings {

        public Launcher launcher = new Launcher();
        public Wine wine = new Wine();
        public Game game = new Game();
        public Api api = new Api();

        public record Launcher {

            public string language = "en-us";
            public bool display_splash_screen = true;

        }

        public record Wine {

            public string prefix_path = Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, "wine");

        }

        public record Game {

            public string server = "global";
            public string installation_path = Path.Join(LauncherConstants.LAUNCHER_HOME_DIRECTORY, "game");

        }

        public record Api {

            public Dictionary<string, CompanyApi> company = new Dictionary<string, CompanyApi> {

                { "global", new CompanyApi() },
                { "chinese", new CompanyApi {

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