using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Wine;

namespace Dodoco.Core.Game {

    public class GameStable: Game {

        public GameStable(Version version, GameServer server, Resource resource, Resource latestResource, IWine wine, string directory, GameState state): base(version, server, resource, latestResource, wine, directory, state) {}
        
    }

}