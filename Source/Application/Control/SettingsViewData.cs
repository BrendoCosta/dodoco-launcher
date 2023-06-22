using Dodoco.Core.Network.Api.Github.Repos.Release;
using Dodoco.Core.Network.HTTP;

namespace Dodoco.Application.Control {

    public record SettingsViewData {

        public Dictionary<string, DownloadProgressReport> WineDownloadStatus { get; set; } = new Dictionary<string, DownloadProgressReport>();

    }

}