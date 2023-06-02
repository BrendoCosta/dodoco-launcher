using Dodoco.Game;
using Dodoco.Launcher;

namespace Dodoco.Network.Controller {

    public sealed class GlobalInstancesController {

        public static ILauncher GetLauncherInstance() {

            return (ILauncher) Launcher.Launcher.GetInstance();

        }

        public static IGame GetGameInstance() {

            return Launcher.Launcher.GetInstance().GetGame();

        }

    }

}