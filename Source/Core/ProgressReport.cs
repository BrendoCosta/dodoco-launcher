namespace Dodoco.Core {

    public record ProgressReport {

        public double completionPercentage { get; set; } = 0.0D;
        public TimeSpan estimatedRemainingTime { get; set; } = TimeSpan.FromSeconds(0.0D);

    }

}