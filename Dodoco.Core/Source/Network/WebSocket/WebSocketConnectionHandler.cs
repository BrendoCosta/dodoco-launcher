using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core.Network.WebSocket {

    public class WebSocketConnectionHandler: IWebSocketConnectionHandler {

        public static readonly int RECEIVE_BUFFER_SIZE = 1024 * 64; // 64 KB
        public static readonly TimeSpan KEEP_ALIVE_INTERVAL = new TimeSpan(0, 0, 1); // 1 second

        public System.Net.WebSockets.WebSocket? Socket;
        public List<Action> OnOpenHandlers = new List<Action>();
        public List<Action> OnCloseHandlers = new List<Action>();
        public List<Action<byte[]>> OnMessageHandlers = new List<Action<byte[]>>();
        public List<Action<WebSocketPacket>> OnPacketHandlers = new List<Action<WebSocketPacket>>();
        public Dictionary<string, Action<WebSocketPacket>> BindHandlers = new Dictionary<string, Action<WebSocketPacket>>();

        public IWebSocketConnectionHandler OnOpen(Action handler) {

            this.OnOpenHandlers.Add(handler);
            return this;

        }

        public IWebSocketConnectionHandler OnClose(Action handler) {

            this.OnCloseHandlers.Add(handler);
            return this;

        }

        public IWebSocketConnectionHandler OnMessage(Action<byte[]> handler) {

            this.OnMessageHandlers.Add(handler);
            return this;

        }

        public IWebSocketConnectionHandler OnPacket(Action<WebSocketPacket> handler) {

            this.OnPacketHandlers.Add(handler);
            return this;

        }

        public IWebSocketConnectionHandler BindPacket(string identifier, Action<WebSocketPacket> handler) {

            this.BindHandlers.Add(identifier, handler);
            return this;

        }

        public IWebSocketConnectionHandler UnbindPacket(string identifier) {

            this.BindHandlers.Remove(identifier);
            return this;

        }

        /*
         * Open
        */

        public IWebSocketConnectionHandler Open(HttpListenerWebSocketContext context) {

            this.Socket = context.WebSocket;
            this.OnOpenHandlers.ForEach((handler) => handler.Invoke());
            return this;

        }

        public IWebSocketConnectionHandler Open(HttpListenerContext context) {

            try {

                if (context.Request.IsWebSocketRequest) {

                    Task<HttpListenerWebSocketContext> task = Task.Run<HttpListenerWebSocketContext>(async () => await context.AcceptWebSocketAsync(null, WebSocketConnectionHandler.RECEIVE_BUFFER_SIZE, WebSocketConnectionHandler.KEEP_ALIVE_INTERVAL));
                    this.Open(task.Result);

                }

            } catch (Exception e) {

                Logger.GetInstance().Error($"Failed to open the WebSocket connection", e);

            }

            return this;

        }

        public IWebSocketConnectionHandler Listen() {

            try {

                Task.WaitAll(this.ListenAsync());

            } catch (Exception e) {

                Logger.GetInstance().Error($"Failed to listen to the WebSocket connection", e);
                this.Close();

            }

            return this;
            
        }

        public async Task<IWebSocketConnectionHandler> ListenAsync() {

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[WebSocketConnectionHandler.RECEIVE_BUFFER_SIZE]);
            WebSocketReceiveResult? result = null;

            while (this.Socket?.State == WebSocketState.Open) {

                using (MemoryStream memoryStream = new MemoryStream()) {

                    do {

                        result = await this.Socket.ReceiveAsync(buffer, CancellationToken.None);
                        if (buffer.Array != null)
                            memoryStream.Write(buffer.Array, buffer.Offset, result.Count);

                    } while (!result.EndOfMessage);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    switch (result.MessageType) {

                        case WebSocketMessageType.Close:

                            this.Close();
                            break;

                        case WebSocketMessageType.Binary:

                            // TODO
                            break;

                        case WebSocketMessageType.Text:

                            this.HandleTextMessage(memoryStream.ToArray());
                            break;

                    }

                }
                
            }

            await this.CloseAsync();
            return this;

        }

        private void HandleTextMessage(byte[] data) {

            // Notify the raw message to the listners

            this.OnMessageHandlers.ForEach((handler) => handler.Invoke(data));

            // Try to parse the raw message into a WebSocketPacket

            string content = Encoding.UTF8.GetString(data);

            if (!string.IsNullOrWhiteSpace(content)) {

                try {

                    // Notify the message to the listners

                    WebSocketPacket packet = JsonSerializer.Deserialize<WebSocketPacket>(content);
                    
                    this.OnPacketHandlers.ForEach((handler) => handler.Invoke(packet));

                    // Notify the packet from message to the listners

                    foreach (KeyValuePair<string, Action<WebSocketPacket>> listner in this.BindHandlers) {

                        if (listner.Key == packet.identifier)
                            listner.Value.Invoke(packet);

                    }

                } catch (JsonException e) {

                    // Do not report any erros

                }

            }

        }

        public IWebSocketConnectionHandler Send(string message) => this.Send(message, CancellationToken.None);
        public IWebSocketConnectionHandler Send(string message, CancellationToken token) {

            return this.Send(Encoding.UTF8.GetBytes(message));

        }

        public IWebSocketConnectionHandler Send(string identifier, string message) => this.Send(identifier, message, CancellationToken.None);
        public IWebSocketConnectionHandler Send(string identifier, string message, CancellationToken token) {

            return this.Send(new WebSocketPacket { identifier = identifier, payload = message }, token);

        }

        public IWebSocketConnectionHandler Send(WebSocketPacket packet) => this.Send(packet, CancellationToken.None);
        public IWebSocketConnectionHandler Send(WebSocketPacket packet, CancellationToken token) {

            return this.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize<WebSocketPacket>(packet)), token);

        }

        public IWebSocketConnectionHandler Send(byte[] data) => this.Send(data, CancellationToken.None);
        public IWebSocketConnectionHandler Send(byte[] data, CancellationToken token) {

            try {

                if (this.Socket != null)
                    Task.WaitAll(this.Socket.SendAsync(data, WebSocketMessageType.Text, true, token));

            } catch (Exception e) {

                Console.WriteLine($"Failed to send WebSocket message", e.Message);
                Console.WriteLine($"Failed to send WebSocket message", e.InnerException?.Message);
                Logger.GetInstance().Error($"Failed to send WebSocket message", e);

            }

            return this;

        }

        /*
         * CLOSE
        */

        public IWebSocketConnectionHandler Close() {

            try {

                Task.WaitAll(this.CloseAsync());

            } catch (Exception e) {

                Logger.GetInstance().Error($"Failed to close the WebSocket connection", e);

            }

            return this;

        }

        public async Task<IWebSocketConnectionHandler> CloseAsync() {

            if (this.Socket?.State == WebSocketState.Open) {

                await this.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

            } else if (this.Socket?.State == WebSocketState.CloseReceived) {

                await this.Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

            } else {

                return this;

            }

            this.OnCloseHandlers.ForEach((handler) => handler.Invoke());
            return this;

        }

        public System.Net.WebSockets.WebSocket? GetSocket() => this.Socket;

    }

}