namespace Dodoco.Core.Network.HTTP {

    public record DownloadProgressReport: ProgressReport {

        public double bytesPerSecond { get; set; } = 0.0D;

    }

}