namespace Dodoco.Core.Protocol.Company.Launcher.Content;

public struct ContentIcon {

    public string icon_id { get; set; }
    public string img { get; set; }
    public string tittle { get; set; }
    public string url { get; set; }
    public string qr_img { get; set; }
    public string qr_desc { get; set; }
    public string img_hover { get; set; }
    public List<ContentOtherLink> other_links { get; set; }
    public string title { get; set; }
    public string icon_link { get; set; }
    public List<ContentLink> links { get; set; }

}