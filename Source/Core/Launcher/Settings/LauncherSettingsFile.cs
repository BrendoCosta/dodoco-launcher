using Dodoco.Core.Serialization.Yaml;

namespace Dodoco.Core.Launcher.Settings {

    public class LauncherSettingsFile: WritableSerializableManagedFile<LauncherSettings> {

        public LauncherSettingsFile(): base(
            "settings",
            LauncherConstants.SETTINGS_DIRECTORY,
            LauncherConstants.SETTINGS_FILENAME,
            new YamlSerializer()
        ) {}

    }

}