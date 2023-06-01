using Dodoco.Application;

namespace Dodoco.Network.HTTP {

    public record DownloadProgressReport: ApplicationProgressReport {

        public double bytesPerSecond { get; set; } = 0.0D;

    }

}