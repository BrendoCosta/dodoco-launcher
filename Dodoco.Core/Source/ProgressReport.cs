namespace Dodoco.Core {

    public record ProgressReport {

        public double Done { get; set; } = 0.0D;
        public double Total { get; set; } = 0.0D;
        public double? Rate { get; set; } = null;
        public TimeSpan? EstimatedRemainingTime { get; set; } = null;
        public string? Message { get; set; } = null;

    }

}