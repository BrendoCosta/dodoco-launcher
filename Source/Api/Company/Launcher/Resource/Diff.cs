namespace Dodoco.Api.Company.Launcher.Resource {
    
    public struct Diff {

        public string name { get; set; }
        public string version { get; set; }
        public string path { get; set; }
        public ulong size { get; set; }
        public string md5 { get; set; }
        public bool is_recommended_update { get; set; }
        public List<VoicePack> voice_packs { get; set; }
        public ulong package_size { get; set; }
        
    }

}