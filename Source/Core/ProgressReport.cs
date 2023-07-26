namespace Dodoco.Core {

    public record ProgressReport {

        public double BytesPerSecond { get; set; } = 0.0D;
        public double BytesTransferred { get; set; } = 0.0D;
        public double TotalBytesTransferred { get; set; } = 0.0D;
        public double CompletionPercentage { get; set; } = 0.0D;
        public TimeSpan EstimatedRemainingTime { get; set; } = TimeSpan.FromSeconds(0.0D);
        public string? Message { get; set; } = null;

    }

}