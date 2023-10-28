namespace Dodoco.Core.Game {

    public struct GameFileIntegrityReportEx {

        public GameFileIntegrityState State { get; init; }
        public string Path { get; init; }
        public string LocalChecksum { get; init; }
        public long LocalSize { get; init; }
        public string RemoteChecksum { get; init; }
        public long RemoteSize { get; init; }


    }

}