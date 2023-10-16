namespace Dodoco.Core.Protocol.Company.Launcher.Resource;

public struct ResourceDiff {

    public string name { get; set; }
    public string version { get; set; }
    public string path { get; set; }
    public long size { get; set; }
    public string md5 { get; set; }
    public bool is_recommended_update { get; set; }
    public List<ResourceVoicePack> voice_packs { get; set; }
    public long package_size { get; set; }
    
}