using Reinforced.Typings.Fluent;

namespace Dodoco.Application.ReinforcedTypings {

    public static class ReinforcedTypingsConfiguration {

        public static void Configure(ConfigurationBuilder builder) {

            // Global configuration

            builder.Global(conf =>
                conf.UseModules(true)
                .AutoOptionalProperties(true)
                .ExportPureTypings(false)
            );

            // Export enums

            builder.ExportAsEnums(
                new Type[] {
                    typeof(Dodoco.Core.Game.GameState),
                    typeof(Dodoco.Core.Launcher.LauncherState),
                    typeof(Dodoco.Core.Util.Log.LogType),
                    typeof(Dodoco.Core.Launcher.LauncherLanguage),
                    typeof(Dodoco.Core.Wine.WineState)
                },
                conf => conf
                    .UseString(true)
            );

            // Export interfaces

            builder.ExportAsInterfaces(
                new Type[] {
                    typeof(Dodoco.Application.Control.MainViewData),
                    typeof(Dodoco.Application.Control.SettingsViewData),
                    typeof(Dodoco.Application.Control.SplashViewData),
                    typeof(Dodoco.Core.ProgressReport),
                    typeof(Dodoco.Core.Game.GameFileIntegrityReport),
                    typeof(Dodoco.Core.Game.IGame),
                    typeof(Dodoco.Core.Launcher.ILauncher),
                    typeof(Dodoco.Core.Launcher.Cache.LauncherCache),
                    typeof(Dodoco.Core.Launcher.Cache.LauncherCache.BackgroundImage),
                    typeof(Dodoco.Core.Launcher.Settings.LauncherSettings),
                    typeof(Dodoco.Core.Launcher.Settings.LauncherSettings.ApiConfig),
                    typeof(Dodoco.Core.Launcher.Settings.LauncherSettings.ApiConfig.CompanyApi),
                    typeof(Dodoco.Core.Launcher.Settings.LauncherSettings.GameConfig),
                    typeof(Dodoco.Core.Launcher.Settings.LauncherSettings.LauncherConfig),
                    typeof(Dodoco.Core.Launcher.Settings.LauncherSettings.WineConfig),
                    typeof(Dodoco.Core.Network.Api.Github.Repos.Release.Asset),
                    typeof(Dodoco.Core.Network.Api.Github.Repos.Release.Author),
                    typeof(Dodoco.Core.Network.Api.Github.Repos.Release.Reactions),
                    typeof(Dodoco.Core.Network.Api.Github.Repos.Release.Release),
                    typeof(Dodoco.Core.Network.Api.Github.Repos.Release.Uploader),
                    typeof(Dodoco.Core.Util.Log.LogEntry),
                    typeof(Dodoco.Core.Wine.IWine)
                },
                conf => conf
                    .WithPublicProperties()
                    .AutoI(false)
                    .WithPublicMethods()
                    .WithPublicMethods(method => method.WithCodeGenerator<ControllerMethodsGenerator>())
            );

            builder.ExportAsClasses(
                new Type[] {
                    typeof(Dodoco.Application.Control.MainController),
                    typeof(Dodoco.Application.Control.SettingsController),
                    typeof(Dodoco.Application.Control.SplashController)
                },
                conf => conf
                    .AddImport("{ Nullable }", "@Dodoco/index")
                    .AddImport("{ RpcClient }", "@Dodoco/Backend")
                    .WithCodeGenerator<ControllerClassGenerator>()
                    .WithPublicMethods(method => method.WithCodeGenerator<ControllerMethodsGenerator>())
            );

        }

    } 

}