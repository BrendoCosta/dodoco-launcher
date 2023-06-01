using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Dodoco.Network.HTTP;
using Dodoco.Util.Log;

namespace Dodoco.Application {

    public sealed class Application {

        private Grapevine.IRestServer? server;
        public DodocoHttpClient client { get; private set; }
        public int port { get; private set; } = ApplicationConstants.DEFAULT_APPLICATION_TCP_PORT;
        private static Application? instance = null;
        
        public string title { get; private set; }
        public string version { get; private set; }
        public string company { get; private set; }
        public string description { get; private set; }

        public ApplicationLog log = new ApplicationLog();

        public static Application GetInstance() {

            if (instance == null) {

                instance = new Application();
            }

            return instance;
            
        }

        private Application() {

            /*
             * Manages application's log file
            */

            if (!this.log.Exists()) {

                this.log.CreateFile();

            }

            this.log.StartWritingToLog();

            /*
             * Load assembly metadata
            */

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

            /*
             * Create the application's home directory if it not exists
            */

            if (Directory.Exists(ApplicationConstants.APPLICATION_HOME_DIRECTORY)) {

                Logger.GetInstance().Log($"Successfully found application's home directory ({ApplicationConstants.APPLICATION_HOME_DIRECTORY})");

            } else {

                Logger.GetInstance().Warning($"The application's home directory ({ApplicationConstants.APPLICATION_HOME_DIRECTORY}) doesn't exists and it will be created.");

                try {

                    Directory.CreateDirectory(ApplicationConstants.APPLICATION_HOME_DIRECTORY);
                    Logger.GetInstance().Log($"Successfully created application's home directory");

                } catch (Exception e) { throw new ApplicationException($"Failed to create application's home directory", e); }

            }

            /*
             * Manages application's HTTP server
            */

            Logger.GetInstance().Log("Starting HTTP server...");

            IConfigurationRoot config = new ConfigurationBuilder().Build();
            
            Action<IServiceCollection> configServices = (services) => {

                services.AddLogging(configure => configure.AddConsole());
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.None);

            };

            this.port = Int32.Parse(Grapevine.PortFinder.FindNextLocalOpenPort(this.port));
            
            Action<Grapevine.IRestServer> configServer = (server) => {

                /*
                 * Manages HTTP server's content-types
                */

                Grapevine.ContentType.Add("js", new Grapevine.ContentType("application/javascript", false, "utf-8"));

                /*
                 * Manages HTTP server's global response headers
                */
                
                server.GlobalResponseHeaders.Add(new Grapevine.GlobalResponseHeaders("Cache-Control", "no-store"));
                
                /*
                 * Manages HTTP server content-folders
                */
                
                string? binaryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

                if (binaryPath != null) {

                    string bundlePath = Path.Combine(binaryPath, ApplicationConstants.APPLICATION_BUNDLE_FOLDER_NAME);

                    if (Directory.Exists(bundlePath)) {

                        server.Prefixes.Add($"http://localhost:{this.port}/");
                        // ---- For debugging ----
                        server.GlobalResponseHeaders.Add(new Grapevine.GlobalResponseHeaders("Access-Control-Allow-Origin", "http://localhost:5173"));
                        // -----------------------

                        /*
                         * NOTE: In order to Grapevine serve files from content folders,
                         * the path must not end with "/" character, otherwise a HTTP 501
                         * message will be sent as the response.
                        */

                        server.ContentFolders.Add(new Grapevine.ContentFolder(bundlePath));
                        server.ContentFolders.Add(new Grapevine.ContentFolder(ApplicationConstants.APPLICATION_HOME_DIRECTORY));
                        Grapevine.MiddlewareExtensions.UseContentFolders(server);
                        server.Router.Options.SendExceptionMessages = true;

                        Logger.GetInstance().Log($"Serving application's bundle's files from {bundlePath}");
                        Logger.GetInstance().Log($"Serving application's home directory's files from {ApplicationConstants.APPLICATION_HOME_DIRECTORY}");

                    } else {

                        throw new ApplicationException($"Application bundle's files' path ({bundlePath}) doesn't exists");

                    }

                } else {

                    throw new ApplicationException($"Failed to get the path of application's executable file");

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

                throw new ApplicationException($"Failed to start HTTP server at TCP port {this.port}", e);

            }

            /*
             * Manages application's HTTP client
            */

            Logger.GetInstance().Log("Starting HTTP client...");
            this.client = new DodocoHttpClient();
            Logger.GetInstance().Log("Successfully started HTTP client");

            Logger.GetInstance().Log("Successfully started application");

        }

        public void End(int exitCode) {

            Logger.GetInstance().Log("Closing application...");

            this.server?.Stop();
            this.client?.Dispose();

            Logger.GetInstance().Log($"Application finished with exit code {exitCode} {(exitCode == 0 ? "(Success)" : "(Error)" )}");

            this.log.StopWritingToLog();
            Console.WriteLine(Dodoco.Util.Log.Logger.GetInstance().GetFullLogText());
            System.Environment.Exit(exitCode);

        }

    }

}
