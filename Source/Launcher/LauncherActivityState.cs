namespace Dodoco.Launcher {

    public enum LauncherActivityState {

        UNREADY,
        READY,
        FETCHING_LAUNCHER_SETTINGS,
        FETCHING_WEB_DATA,
        NEEDS_GAME_DOWNLOAD,
        NEEDS_GAME_UPDATE,
        READY_TO_PLAY,
        CHECKING_GAME_INTEGRITY,

    }

}