namespace Dodoco.Game {

    public class GameException: Dodoco.Application.ApplicationException {

        public GameException(): base() {}
        public GameException(string? message): base(message) {}
        public GameException(string? message, Exception? innerException): base(message, innerException) {}

    }

}