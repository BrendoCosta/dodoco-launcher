namespace Dodoco.Core.Game {

    public enum GameState {

        DOWNLOADING,
        RECOVERING_DOWNLOADED_SEGMENTS,
        EXTRACTING_DOWNLOADED_SEGMENTS,
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