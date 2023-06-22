using Dodoco.Core.Serialization.Json;

namespace Dodoco.Core.Launcher.Cache {

    public class LauncherCacheFile: FormatFile<LauncherCache> {

        public LauncherCacheFile(): base(
            "cache",
            LauncherConstants.CACHE_DIRECTORY,
            LauncherConstants.CACHE_FILENAME,
            new JsonSerializer()
        ) {}
        
    }

}