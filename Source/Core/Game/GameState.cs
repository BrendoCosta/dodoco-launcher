namespace Dodoco.Core.Game {

    public enum GameState {

        NOT_INSTALLED,
        WAITING_FOR_DOWNLOAD,
        WAITING_FOR_UPDATE,
        DOWNLOADING,
        // Updating
        DOWNLOADING_UPDATE,
        EXTRACTING_UPDATE,
        PATCHING_FILES,
        REMOVING_DEPRECATED_FILES,
        // Running
        READY,
        RUNNING,
        // Others
        CHECKING_INTEGRITY,
        REPAIRING_FILES

    }

}