namespace Dodoco.Core.Game;

using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Serialization.Json;

using System.Text;

public class GameResourceCacheFile: WritableSerializableManagedFile<List<GameResourceCache>> {

    public GameResourceCacheFile(): base(
        "resource cache",
        Constants.CACHE_DIRECTORY,
        $"resource.json",
        new JsonSerializer()
    ) {}
    
}