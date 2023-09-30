namespace Dodoco.Core.Wine {

    public record WineSettings {

        public bool EnableCustomWineInstallation { get; set; } = false;
        public string? CustomWineDirectory { get; set; }
        public string SelectedRelease { get; set; } = "GE-Proton8-8";
        public string ReleasesDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "wine");
        public string PrefixDirectory { get; set; } = Path.Join(Constants.HOME_DIRECTORY, "wine", "prefix");

    }

}