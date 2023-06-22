using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Wine;

namespace Dodoco.Core.Game {

    public class GameStable: Game {

        public GameStable(Version version, GameServer server, Resource resource, IWine wine, string directory, GameState state): base(version, server, resource, wine, directory, state) {}
        
    }

}