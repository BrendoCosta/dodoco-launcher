using Dodoco.Game;
using Dodoco.Launcher;
using Dodoco.Network.WebSocket;
using Dodoco.Util.Log;

using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using System.Buffers;
using System.Net.WebSockets;

namespace Dodoco.Network.Controller {

    public sealed class RpcGlobalInstancesController {

        public ILauncher GetLauncherInstance() {

            return (ILauncher) Launcher.Launcher.GetInstance();

        }

        public IGame GetGameInstance() {

            return Launcher.Launcher.GetInstance().GetGame();

        }

    }

    [Grapevine.RestResource(BasePath = "/Dodoco/Network/Controller/RpcController")]
    public sealed class RpcController {

        [Grapevine.RestRoute("GET", "/")]
        public async Task HandleRPCWebSocketConnection(Grapevine.IHttpContext context) {

            System.Net.HttpListenerContext baseContext = ((Grapevine.HttpContext) context).Advanced;
            
            if (baseContext.Request.IsWebSocketRequest) {

                HttpListenerWebSocketContext webSocketContext = await baseContext.AcceptWebSocketAsync(null);

                JsonMessageFormatter jsonFormatter = new JsonMessageFormatter();
                jsonFormatter.JsonSerializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                jsonFormatter.JsonSerializer.Converters.Add(new Newtonsoft.Json.Converters.VersionConverter());

                JsonRpc jsonRpc = new StreamJsonRpc.JsonRpc(new StreamJsonRpc.WebSocketMessageHandler(webSocketContext.WebSocket, jsonFormatter));
                jsonRpc.CancelLocallyInvokedMethodsWhenConnectionIsClosed = true;
                
                jsonRpc.AddLocalRpcTarget((ILauncher) Launcher.Launcher.GetInstance(), new StreamJsonRpc.JsonRpcTargetOptions() {
                    MethodNameTransform = (string methodName) => $"Dodoco.Launcher.ILauncher.{methodName}"
                });

                jsonRpc.AddLocalRpcTarget(Logger.GetInstance(), new StreamJsonRpc.JsonRpcTargetOptions() {
                    MethodNameTransform = (string methodName) => $"Dodoco.Util.Log.Logger.{methodName}"
                });

                jsonRpc.AddLocalRpcTarget(new RpcGlobalInstancesController(), new StreamJsonRpc.JsonRpcTargetOptions() {
                    MethodNameTransform = (string methodName) => $"Dodoco.Network.Controller.RpcGlobalInstancesController.{methodName}"
                });

                jsonRpc.StartListening();

                while (webSocketContext.WebSocket.State != WebSocketState.Closed) {

                    await Task.Delay(50);

                }

            }

        }

    }

}