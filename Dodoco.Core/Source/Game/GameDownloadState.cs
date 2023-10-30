
namespace Dodoco.Core.Game {

    [ObsoleteAttribute("This property is obsolete. Use GameInstallationManagerState instead.", false)]
    public enum GameDownloadState {

        DOWNLOADED,
        RECOVERING_DOWNLOADED_SEGMENTS,
        DOWNLOADING_SEGMENTS,
        UNZIPPING_SEGMENTS

    }

}