namespace Dodoco.Api.Company.Launcher.Resource {

    public struct Latest {

        public string name { get; set; }
        public string version { get; set; }
        public string path { get; set; }
        public ulong size { get; set; }
        public string md5 { get; set; }
        public string entry { get; set; }
        public List<VoicePack> voice_packs { get; set; }
        public string decompressed_path { get; set; }
        public List<Segment> segments { get; set; }
        public ulong package_size { get; set; }
        
    }

}