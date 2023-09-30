using Dodoco.Application.Control;
using Dodoco.Core;
using Dodoco.Core.Launcher;
using Dodoco.Core.Util.Log;

using Grapevine;
using StreamJsonRpc;
using System.Drawing;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dodoco.Application {

    public sealed class Application {

        public int Port { get; private set; } = ApplicationConstants.DEFAULT_APPLICATION_TCP_PORT;
        public string Title { get; private set; }
        public string Version { get; private set; }
        public string Company { get; private set; }
        public string Description { get; private set; }

        private LogFile logFile = new LogFile();
        private Grapevine.IRestServer? server;

        public Application() {

            /*
             * Load assembly metadata
            */

            AssemblyProductAttribute? productAttribute         = (AssemblyProductAttribute?) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false);
            AssemblyTitleAttribute? titleAttribute             = (AssemblyTitleAttribute?) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false);
            AssemblyFileVersionAttribute? fileVersionAttribute = (AssemblyFileVersionAttribute?) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute), false);
            AssemblyCompanyAttribute? companyAttribute         = (AssemblyCompanyAttribute?) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute), false);
            AssemblyDescriptionAttribute? descriptionAttribute = (AssemblyDescriptionAttribute?) Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute), false);

            this.Title = productAttribute?.Product ?? "[Assembly product attribute not found]";
            this.Version = fileVersionAttribute?.Version ?? "[Assembly file version attribute not found]";
            this.Company = companyAttribute?.Company ?? "[Assembly company attribute not found]";
            this.Description = descriptionAttribute?.Description ?? "[Assembly description attribute not found]";

        }

        static async Task Main(string[] args) {

            Application app = new Application();
            await app.Run();

        }

        public async Task Run() {

            if (!this.logFile.Exist())
                this.logFile.Create();

            this.logFile.StartWritingToLog();

            Logger.GetInstance().Log($"{this.Title} - {this.Company}");
            Logger.GetInstance().Log($"Version: {this.Version} ({this.Description})");
            Logger.GetInstance().Log($"Identifier: {Constants.IDENTIFIER}");
            Logger.GetInstance().Log("Starting application...");

            IApplicationWindow window = new ApplicationWindow();
            Launcher launcher = new Launcher();

            launcher.OnStateUpdate += (object? sender, LauncherState e) => {

                if (e == LauncherState.READY) {

                    window.SetResizable(true);
                    window.SetSize(new Size(1270, 766));

                }

            };

            window.SetTitle(this.Title);
            window.SetSize(new Size(300, 400));
            window.SetResizable(false);
            window.SetFrameless(false);
            window.OnClose += new EventHandler((object? sender, EventArgs e) => launcher.Stop());

            /*
             * Manages application's HTTP server
            */

            Logger.GetInstance().Log("Starting HTTP server...");

            IConfigurationRoot config = new ConfigurationBuilder().Build();
            
            Action<IServiceCollection> configServices = (services) => {

                services.AddLogging(configure => configure.AddConsole());
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.None);

            };

            this.Port = Int32.Parse(Grapevine.PortFinder.FindNextLocalOpenPort(this.Port));
            string url = $"http://localhost:{this.Port}/";
            
            Action<Grapevine.IRestServer> configServer = (server) => {

                /*
                 * Manages WebSocket RPC route
                */

                server.Router.Register(new Route(async (context) => {

                    HttpListenerContext baseContext = ((HttpContext) context).Advanced;
                    
                    if (baseContext.Request.IsWebSocketRequest) {

                        HttpListenerWebSocketContext webSocketContext = await baseContext.AcceptWebSocketAsync(null);

                        JsonMessageFormatter jsonFormatter = new JsonMessageFormatter();
                        jsonFormatter.JsonSerializer.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
                        jsonFormatter.JsonSerializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        jsonFormatter.JsonSerializer.Converters.Add(new Newtonsoft.Json.Converters.VersionConverter());

                        JsonRpc jsonRpc = new StreamJsonRpc.JsonRpc(new StreamJsonRpc.WebSocketMessageHandler(webSocketContext.WebSocket, jsonFormatter));
                        jsonRpc.CancelLocallyInvokedMethodsWhenConnectionIsClosed = true;

                        /*
                         * Manages RPC controllers
                        */
                        
                        jsonRpc.AddLocalRpcTarget(new LauncherController(launcher), new StreamJsonRpc.JsonRpcTargetOptions() {
                            MethodNameTransform = (string methodName) => $"{typeof(LauncherController).FullName}.{methodName}"
                        });

                        jsonRpc.AddLocalRpcTarget(new GameController(launcher), new StreamJsonRpc.JsonRpcTargetOptions() {
                            MethodNameTransform = (string methodName) => $"{typeof(GameController).FullName}.{methodName}"
                        });

                        jsonRpc.AddLocalRpcTarget(new MainController(), new StreamJsonRpc.JsonRpcTargetOptions() {
                            MethodNameTransform = (string methodName) => $"{typeof(MainController).FullName}.{methodName}"
                        });

                        jsonRpc.AddLocalRpcTarget(new WineController(launcher), new StreamJsonRpc.JsonRpcTargetOptions() {
                            MethodNameTransform = (string methodName) => $"{typeof(WineController).FullName}.{methodName}"
                        });

                        jsonRpc.AddLocalRpcTarget(new SplashController(), new StreamJsonRpc.JsonRpcTargetOptions() {
                            MethodNameTransform = (string methodName) => $"{typeof(SplashController).FullName}.{methodName}"
                        });

                        //jsonRpc.AddLocalRpcTarget(Logger.GetInstance(), new StreamJsonRpc.JsonRpcTargetOptions() {
                        //    MethodNameTransform = (string methodName) => $"{typeof(Logger).FullName}.{methodName}",
                        //    NotifyClientOfEvents = false
                        //});

                        jsonRpc.StartListening();

                        while (webSocketContext.WebSocket.State != WebSocketState.Closed) {

                            await Task.Delay(50);

                        }

                    }

                }, "GET", "/rpc"));
                
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
                
                string? binaryPath = Path.GetDirectoryName(AppContext.BaseDirectory);

                if (binaryPath != null) {

                    string bundlePath = Path.Combine(binaryPath, ApplicationConstants.BUNDLE_FOLDER_NAME);

                    if (Directory.Exists(bundlePath)) {

                        server.Prefixes.Add(url);
                        // ---- For development ----
                        server.GlobalResponseHeaders.Add(new Grapevine.GlobalResponseHeaders("Access-Control-Allow-Origin", "http://localhost:5173"));
                        // -----------------------

                        /*
                         * NOTE: In order to Grapevine serve files from content folders,
                         * the path must not end with "/" character, otherwise a HTTP 501
                         * message will be sent as the response.
                        */

                        server.ContentFolders.Add(new Grapevine.ContentFolder(bundlePath));
                        server.ContentFolders.Add(new Grapevine.ContentFolder(Constants.HOME_DIRECTORY));
                        Grapevine.MiddlewareExtensions.UseContentFolders(server);
                        server.Router.Options.SendExceptionMessages = true;

                        Logger.GetInstance().Log($"Serving application's bundle's files from {bundlePath}");
                        Logger.GetInstance().Log($"Serving application's home directory's files from {Constants.HOME_DIRECTORY}");

                    } else {

                        throw new CoreException($"Application bundle's files' path ({bundlePath}) doesn't exists");

                    }

                } else {

                    throw new CoreException($"Failed to get the path of application's executable file");

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

            } catch (Exception e) {

                throw new CoreException($"Failed to start HTTP server at TCP port {this.Port}", e);

            }

            // Run

            /*
             * Resize the screen after launcher starts
            */
           
            window.SetUri(new Uri(url));
            window.Open();
            launcher.Start();

            while (window.IsOpen()) {

                Thread.Sleep(500);

            }

            this.Exit(0);

        }

        public void Exit(int exitCode) {

            Logger.GetInstance().Log("Closing application...");
            this.server?.Stop();

            Logger.GetInstance().Log($"Application finished with exit code {exitCode} {(exitCode == 0 ? "(Success)" : "(Error)" )}");
            this.logFile.StopWritingToLog();
            
            Environment.Exit(exitCode);

        }

    }

}
