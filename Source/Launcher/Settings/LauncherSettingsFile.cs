using Dodoco.Application;
using Dodoco.Game;
using Dodoco.Serialization.Yaml;

using System.Globalization;

namespace Dodoco.Launcher.Settings {

    public record LauncherSettingsFile: ApplicationFile<LauncherSettings> {

        public LauncherSettingsFile(): base(
            "settings",
            ApplicationConstants.APPLICATION_HOME_DIRECTORY,
            LauncherConstants.LAUNCHER_SETTINGS_FILENAME,
            new YamlSerializer()
        ) {}

    }

}