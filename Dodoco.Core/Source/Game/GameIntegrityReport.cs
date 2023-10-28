namespace Dodoco.Core.Game {

    [ObsoleteAttribute("This property is obsolete. Use GameFileIntegrityReportEx instead.", false)]
    public struct GameFileIntegrityReport {

        public GameFileIntegrityState localFileIntegrityState { get; set; }
        public string localFilePath { get; set; }
        public string localFileHash { get; set; }
        public ulong localFileSize { get; set; }
        public string remoteFilePath { get; set; }
        public string remoteFileHash { get; set; }
        public ulong remoteFileSize { get; set; }

    }

}