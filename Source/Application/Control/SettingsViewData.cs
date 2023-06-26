using Dodoco.Core;

namespace Dodoco.Application.Control {

    public record SettingsViewData {

        public Dictionary<string, ProgressReport> WineDownloadStatus { get; set; } = new Dictionary<string, ProgressReport>();

    }

}