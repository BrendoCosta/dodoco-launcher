namespace Dodoco.Core.Protocol.Company.Launcher.Content;

public struct ContentData {

    public ContentAdv adv { get; set; }
    public List<ContentBanner> banner { get; set; }
    public List<ContentIcon> icon { get; set; }
    public List<ContentPost> post { get; set; }
    public List<object> qq { get; set; }
    public ContentMore more { get; set; }
    public ContentLinks links { get; set; }

}