using Dodoco.Application;
using Dodoco.Network;

using Dodoco.Network.HTTP;
using Dodoco.Serialization.Yaml;
using Dodoco.Util.Hash;
using Dodoco.Util.Log;
using Dodoco.Util.Unit;

using System.Text.RegularExpressions;

namespace Dodoco.Launcher.Cache {

    public record LauncherCacheFile: ApplicationFile<LauncherCache> {

        public LauncherCacheFile(): base(
            "cache",
            LauncherConstants.LAUNCHER_CACHE_DIRECTORY,
            LauncherConstants.LAUNCHER_CACHE_FILENAME,
            new YamlSerializer()
        ) {}
        
    }

}