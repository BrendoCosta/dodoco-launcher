using Dodoco.Launcher;
using Dodoco.Util.Log;
using System.Text;
using System.Text.RegularExpressions;

namespace Dodoco.Game.Stable {

    public class Game: IGame {

        private static Game? instance = null;
        private static Version UNINITIALIZED_VERSION = Version.Parse("9.9.9");
        public Version version = UNINITIALIZED_VERSION;
        
        public Game() {}
        
        public static Game GetInstance() {

            if (instance == null) {

                instance = new Game();

            }

            return instance;
            
        }

        public Version GetVersion() {

            if (this.version == UNINITIALIZED_VERSION) {

                Logger.GetInstance().Warning("Launcher is getting the stable version of the game without updating stable version definition. This indicates a logical mistake");

            }

            return this.version;

        }

        public void SetVersion(Version version) {

            this.version = version;

        }

    }

}