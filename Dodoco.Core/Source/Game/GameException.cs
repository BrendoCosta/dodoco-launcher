namespace Dodoco.Core.Game {

    public class GameException: CoreException {

        public GameException(): base() {}
        public GameException(string? message): base(message) {}
        public GameException(string? message, Exception? innerException): base(message, innerException) {}

    }

}