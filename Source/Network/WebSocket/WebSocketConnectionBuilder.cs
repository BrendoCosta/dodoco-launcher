namespace Dodoco.Network.WebSocket {

    public class WebSocketConnectionBuilder {

        private IWebSocketConnectionHandler ConnectionHandler;

        public WebSocketConnectionBuilder() {

            this.ConnectionHandler = new WebSocketConnectionHandler();

        }

        public IWebSocketConnectionHandler Build() {

            return this.ConnectionHandler;

        }

    }

}