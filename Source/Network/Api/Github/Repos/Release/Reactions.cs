using System.Text.Json.Serialization;

namespace Dodoco.Network.Api.Github.Repos.Release {

    public struct Reactions {

        public string url { get; set; }
        public int total_count { get; set; }
        
        [JsonPropertyName("+1")]
        public int plus_1 { get; set; }

        [JsonPropertyName("-1")]
        public int minus_1 { get; set; }

        public int laugh { get; set; }
        public int hooray { get; set; }
        public int confused { get; set; }
        public int heart { get; set; }
        public int rocket { get; set; }
        public int eyes { get; set; }

    }

}