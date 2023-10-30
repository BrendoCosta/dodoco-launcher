namespace Dodoco.Core.Game;

public struct GameLanguage {

    public string Code { get; set; }
    public string Name { get; set; }

    public static GameLanguage Default = GameLanguage.EnglishUS;
    public static GameLanguage EnglishUS = new GameLanguage { Code = "en-US", Name = "English(US)" };
    public static GameLanguage Chinese = new GameLanguage { Code = "zh-CN", Name = "Chinese" };
    public static GameLanguage Japanese = new GameLanguage { Code = "ja-JP", Name = "Japanese" };
    public static GameLanguage Korean = new GameLanguage { Code = "ko-KR", Name = "Korean" };
    public static List<GameLanguage> All = new List<GameLanguage> {
        
        GameLanguage.EnglishUS,
        GameLanguage.Chinese,
        GameLanguage.Japanese,
        GameLanguage.Korean

    };

}