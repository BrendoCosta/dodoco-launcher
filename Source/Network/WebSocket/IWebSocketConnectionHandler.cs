using System.Net;
using System.Net.WebSockets;

namespace Dodoco.Network.WebSocket {

    public interface IWebSocketConnectionHandler {

        IWebSocketConnectionHandler Open(HttpListenerWebSocketContext context);
        IWebSocketConnectionHandler Open(HttpListenerContext context);
        IWebSocketConnectionHandler Close();
        Task<IWebSocketConnectionHandler> CloseAsync();
        IWebSocketConnectionHandler Listen();
        Task<IWebSocketConnectionHandler> ListenAsync();
        System.Net.WebSockets.WebSocket? GetSocket();

        IWebSocketConnectionHandler OnOpen(Action handler);
        IWebSocketConnectionHandler OnClose(Action handler);
        IWebSocketConnectionHandler OnMessage(Action<byte[]> handler);
        IWebSocketConnectionHandler OnPacket(Action<WebSocketPacket> handler);
        IWebSocketConnectionHandler BindPacket(string identifier, Action<WebSocketPacket> handler);
        IWebSocketConnectionHandler UnbindPacket(string identifier);
        IWebSocketConnectionHandler Send(string message);
        IWebSocketConnectionHandler Send(string message, CancellationToken token = default);
        IWebSocketConnectionHandler Send(string identifier, string message);
        IWebSocketConnectionHandler Send(string identifier, string message, CancellationToken token = default);
        IWebSocketConnectionHandler Send(WebSocketPacket packet);
        IWebSocketConnectionHandler Send(WebSocketPacket packet, CancellationToken token = default);
        IWebSocketConnectionHandler Send(byte[] data);
        IWebSocketConnectionHandler Send(byte[] data, CancellationToken token = default);

    }

}