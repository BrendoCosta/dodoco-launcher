using Reinforced.Typings.Fluent;

namespace Dodoco {

    public static class ReinforcedTypingsConfiguration {

        public static void Configure(ConfigurationBuilder builder) {

            // Global configuration

            builder.Global(conf =>
                conf.UseModules(true)
                .AutoOptionalProperties(true)
                .ExportPureTypings(false)
                .CamelCaseForMethods(true)
            );

            // Export enums

            builder.ExportAsEnums(
                new Type[] {
                    typeof(Dodoco.Game.GameState),
                    typeof(Dodoco.Launcher.LauncherActivityState),
                    typeof(Dodoco.Launcher.LauncherExecutionState),
                    typeof(Dodoco.Util.Log.LogType),
                    typeof(Dodoco.Launcher.LauncherLanguage)
                },
                conf => conf
                    .UseString(true)
            );

            // Export interfaces

            builder.ExportAsInterfaces(
                new Type[] {
                    typeof(Dodoco.Application.ApplicationProgressReport),
                    typeof(Dodoco.Game.GameIntegrityReport),
                    typeof(Dodoco.Game.IGame),
                    typeof(Dodoco.Launcher.ILauncher),
                    typeof(Dodoco.Launcher.Cache.LauncherCache),
                    typeof(Dodoco.Launcher.Cache.LauncherCache.BackgroundImage),
                    typeof(Dodoco.Launcher.Settings.LauncherSettings),
                    typeof(Dodoco.Launcher.Settings.LauncherSettings.Api),
                    typeof(Dodoco.Launcher.Settings.LauncherSettings.Api.CompanyApi),
                    typeof(Dodoco.Launcher.Settings.LauncherSettings.Game),
                    typeof(Dodoco.Launcher.Settings.LauncherSettings.Launcher),
                    typeof(Dodoco.Launcher.Settings.LauncherSettings.Wine),
                    typeof(Dodoco.Network.HTTP.DownloadProgressReport),
                    typeof(Dodoco.Util.Log.LogEntry)
                },
                conf => conf
                    .WithPublicProperties()
                    .AutoI(false)
            );

        }

    } 

}