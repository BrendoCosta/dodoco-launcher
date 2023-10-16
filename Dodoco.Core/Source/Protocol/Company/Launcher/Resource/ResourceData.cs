namespace Dodoco.Core.Protocol.Company.Launcher.Resource;

public struct ResourceData {

    public ResourceGame game { get; set; }
    public ResourcePlugin plugin { get; set; }
    public string web_url { get; set; }
    public object force_update { get; set; }
    public ResourceGame? pre_download_game { get; set; }
    public List<ResourceDeprecatedPackage> deprecated_packages { get; set; }
    public object sdk { get; set; }
    public List<ResourceDeprecatedFile> deprecated_files { get; set; }

}