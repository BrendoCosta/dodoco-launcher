using Dodoco.Core.Serialization.Yaml;

namespace Dodoco.Core.Launcher.Settings {

    public class LauncherSettingsFile: FormatFile<LauncherSettings> {

        public LauncherSettingsFile(): base(
            "settings",
            LauncherConstants.SETTINGS_DIRECTORY,
            LauncherConstants.SETTINGS_FILENAME,
            new YamlSerializer()
        ) {}

    }

}