namespace Dodoco.Api.Company.Launcher.Resource {

    public struct Data {

        public Game game { get; set; }
        public Plugin plugin { get; set; }
        public string web_url { get; set; }
        public object force_update { get; set; }
        public object pre_download_game { get; set; }
        public List<DeprecatedPackage> deprecated_packages { get; set; }
        public object sdk { get; set; }
        public List<DeprecatedFile> deprecated_files { get; set; }

    }

}