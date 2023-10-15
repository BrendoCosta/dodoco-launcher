namespace Dodoco.Core.Game;

public struct GameLanguage {

    public string Code { get; set; }
    public string Name { get; set; }

    public static GameLanguage Default = GameLanguage.EnglishUS;
    public static GameLanguage EnglishUS = new GameLanguage { Code = "en-us", Name = "English(US)" };
    public static GameLanguage Chinese = new GameLanguage { Code = "zh-cn", Name = "Chinese" };
    public static GameLanguage Japanese = new GameLanguage { Code = "ja-jp", Name = "Japanese" };
    public static GameLanguage Korean = new GameLanguage { Code = "ko-kr", Name = "Korean" };
    public static List<GameLanguage> All = new List<GameLanguage> {
        
        GameLanguage.EnglishUS,
        GameLanguage.Chinese,
        GameLanguage.Japanese,
        GameLanguage.Korean

    };

}