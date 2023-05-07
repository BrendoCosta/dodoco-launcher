using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Dococo.Util.Log;

namespace Dodoco.Application {

    public sealed class Application {

        private Grapevine.IRestServer? server;
        public HttpClient client { get; private set; }
        public int port { get; private set; } = ApplicationConstants.DEFAULT_APPLICATION_TCP_PORT;
        private static Application? instance = null;
        public string title { get; private set; }
        public string version { get; private set; }
        public string company { get; private set; }
        public string description { get; private set; }

        public static Application GetInstance() {

            if (instance == null) {

                instance = new Application();
            }

            return instance;
            
        }

        private Application() {

            // Loads assembly metadata

            AssemblyProductAttribute? productAttribute         = (AssemblyProductAttribute?) Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Reflection.AssemblyProductAttribute), false);
            AssemblyTitleAttribute? titleAttribute             = (AssemblyTitleAttribute?) Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Reflection.AssemblyTitleAttribute), false);
            AssemblyFileVersionAttribute? fileVersionAttribute = (AssemblyFileVersionAttribute?) Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Reflection.AssemblyFileVersionAttribute), false);
            AssemblyCompanyAttribute? companyAttribute         = (AssemblyCompanyAttribute?) Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Reflection.AssemblyCompanyAttribute), false);
            AssemblyDescriptionAttribute? descriptionAttribute = (AssemblyDescriptionAttribute?) Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Reflection.AssemblyDescriptionAttribute), false);

            this.title = productAttribute?.Product ?? "[Assembly product attribute not found]";
            this.version = fileVersionAttribute?.Version ?? "[Assembly file version attribute not found]";
            this.company = companyAttribute?.Company ?? "[Assembly company attribute not found]";
            this.description = descriptionAttribute?.Description ?? "[Assembly description attribute not found]";
            
            Logger.GetInstance().Log($"{this.title} - {this.company}");
            Logger.GetInstance().Log($"Version: {this.version} ({this.description})");
            Logger.GetInstance().Log($"Identifier: {ApplicationConstants.DEFAULT_APPLICATION_IDENTIFIER}");
            Logger.GetInstance().Log("Starting application...");

            this.StartServer();
            this.StartClient();

            Logger.GetInstance().Log("Successfully started application");

        }

        private void StartServer() {

            Logger.GetInstance().Log("Starting HTTP server...");

            IConfigurationRoot config = new ConfigurationBuilder().Build();
            
            Action<IServiceCollection> configServices = (services) => {

                services.AddLogging(configure => configure.AddConsole());
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.None);

            };

            this.port = Int32.Parse(Grapevine.PortFinder.FindNextLocalOpenPort(this.port));
            
            Action<Grapevine.IRestServer> configServer = (server) => {

                Grapevine.ContentType.Add("js", new Grapevine.ContentType("application/javascript", false, "utf-8"));

                string? binaryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

                if (binaryPath != null) {

                    string bundlePath = Path.Combine(binaryPath, ApplicationConstants.APPLICATION_BUNDLE_FOLDER_NAME);

                    if (Directory.Exists(bundlePath)) {

                        Logger.GetInstance().Log($"Serving application's bundle's files from {bundlePath}");

                        server.Prefixes.Add($"http://localhost:{this.port}/");
                        // ---- For debugging ----
                        server.GlobalResponseHeaders.Add(new Grapevine.GlobalResponseHeaders("Access-Control-Allow-Origin", "http://localhost:5173"));
                        // -----------------------
                        server.ContentFolders.Add(new Grapevine.ContentFolder(bundlePath));
                        Grapevine.MiddlewareExtensions.UseContentFolders(server);
                        server.Router.Options.SendExceptionMessages = true;

                    } else {

                        Logger.GetInstance().Error($"Application bundle's files' path ({bundlePath}) doesn't exists");
                        this.End(1);

                    }

                } else {

                    Logger.GetInstance().Error($"Failed to get the path of application's executable file");
                    this.End(1);

                }
                
            };

            this.server = new Grapevine.RestServerBuilder(
                new ServiceCollection(),
                config,
                configServices,
                configServer
            ).Build();

            try {

                this.server.Start();
                Logger.GetInstance().Log($"Successfully started HTTP server (listening on {this.server.Prefixes.First()})");

            } catch (System.Exception e) {

                Logger.GetInstance().Error($"Failed to start HTTP server at TCP port {this.port}", e);
                this.End(1);

            }

        }

        public void StartClient() {

            Logger.GetInstance().Log("Starting HTTP client...");
            
            this.client = new HttpClient();

            Logger.GetInstance().Log("Successfully started HTTP client");

        }

        public void End(int exitCode) {

            Logger.GetInstance().Log("Closing application...");

            this.server?.Stop();
            this.client?.Dispose();

            Logger.GetInstance().Log($"Application finished with exit code {exitCode} {(exitCode == 0 ? "(Success)" : "(Error)" )}");

            Console.WriteLine(Dococo.Util.Log.Logger.GetInstance().GetFullLogText());
            System.Environment.Exit(exitCode);

        }

    }

}
