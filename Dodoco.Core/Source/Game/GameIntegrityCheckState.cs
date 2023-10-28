namespace Dodoco.Core.Game {

    [ObsoleteAttribute("This property is obsolete. Use GameIntegrityManagerState instead.", false)]
    public enum GameIntegrityCheckState {

        IDLE,
        CHECKING_INTEGRITY,
        DOWNLOADING_FILE,
        REPAIRING_FILE

    }

}