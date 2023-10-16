namespace Dodoco.Core.Protocol.Company.Launcher.Resource;

public struct ResourceLatest {

    public string name { get; set; }
    public string version { get; set; }
    public string path { get; set; }
    public long size { get; set; }
    public string md5 { get; set; }
    public string entry { get; set; }
    public List<ResourceVoicePack> voice_packs { get; set; }
    public Uri decompressed_path { get; set; }
    public List<ResourceSegment> segments { get; set; }
    public long package_size { get; set; }
    
}