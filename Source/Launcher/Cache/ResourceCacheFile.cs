using Dodoco.Application;
using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Serialization.Json;

namespace Dodoco.Launcher.Cache {

    public record ResourceCacheFile: ApplicationFile<Resource> {

        public ResourceCacheFile(): base(
            "resource cache",
            LauncherConstants.LAUNCHER_CACHE_DIRECTORY,
            LauncherConstants.LAUNCHER_RESOURCE_CACHE_FILENAME,
            new JsonSerializer()
        ) {}

    }

}