namespace Dodoco.Core.Game {

    public class ForbiddenGameStateException: GameException {

        public ForbiddenGameStateException(GameState state): base($"Forbidden state \"{state.ToString()}\"") {}

    }

}