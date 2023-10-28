using Dodoco.Core.Serialization.Json;

namespace Dodoco.Core.Launcher.Cache {

    public class LauncherCacheFile: WritableSerializableManagedFile<LauncherCache> {

        public LauncherCacheFile(): base(
            "cache",
            Constants.CACHE_DIRECTORY,
            LauncherConstants.CACHE_FILENAME,
            new JsonSerializer()
        ) {}
        
    }

}