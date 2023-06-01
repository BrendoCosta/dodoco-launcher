namespace Dodoco.Network.Api.Github.Repos.Content {

    public struct Content {

        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public ulong size { get; set; }
        public Uri url { get; set; }
        public Uri html_url { get; set; }
        public Uri git_url { get; set; }
        public Uri download_url { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public string encoding { get; set; }
        public Links _links { get; set; }

    }

    public struct Links {

        public Uri self { get; set; }
        public Uri git { get; set; }
        public Uri html { get; set; }

    }

}