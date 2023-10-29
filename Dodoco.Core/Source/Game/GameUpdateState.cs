
namespace Dodoco.Core.Game {

    [ObsoleteAttribute("This property is obsolete. Use GameUpdateManagerState instead.", false)]
    public enum GameUpdateState {

        UPDATED,
        DOWNLOADING_UPDATE_PACKAGE,
        UNZIPPING_UPDATE_PACKAGE,
        APPLYING_UPDATE_PACKAGE,
        REMOVING_DEPRECATED_FILES

    }

}