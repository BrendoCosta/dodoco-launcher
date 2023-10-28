namespace Dodoco.Core.Test.Game;

using Dodoco.Core.Game;
using Dodoco.Core.Serialization.Json;
using NUnit.Framework;

[TestFixture]
public class PkgVersionParserTest {

    [Test]
    public void PkgVersionParser_Parsing_Single_Line_Test() {

        string text = "{\"remoteName\": \"GenshinImpact_Data/Managed/Metadata/global-metadata.dat\", \"md5\": \"b6d43e6f1838dde18ef7b98099840098\", \"fileSize\": 58980820}";
        GamePkgVersionEntry result = PkgVersionParser.Parse(text)[0];
        GamePkgVersionEntry expected = new GamePkgVersionEntry {

            remoteName = "GenshinImpact_Data/Managed/Metadata/global-metadata.dat",
            md5 = "b6d43e6f1838dde18ef7b98099840098",
            fileSize = 58980820L

        };

        JsonSerializer serializer = new JsonSerializer();
        Assert.That(serializer.Serialize(result), Is.EqualTo(serializer.Serialize(expected)));

    }

    [Test]
    public void PkgVersionParser_Parsing_Multiline_Test() {

        string text = string.Join("\r\n", new List<string> {
            "{\"remoteName\": \"GenshinImpact_Data/Managed/Metadata/global-metadata.dat\", \"md5\": \"b6d43e6f1838dde18ef7b98099840098\", \"fileSize\": 58980820}",
            "{\"remoteName\": \"GenshinImpact_Data/Managed/Resources/mscorlib.dll-resources.dat\", \"md5\": \"21d06dbc8af6432b2b49536ed30609af\", \"fileSize\": 337563}",
            "{\"remoteName\": \"GenshinImpact_Data/Managed/Resources/Newtonsoft.Json.dll-resources.dat\", \"md5\": \"ef68c753a3826e16a982d610340a3484\", \"fileSize\": 639}"
        });

        List<GamePkgVersionEntry> result = PkgVersionParser.Parse(text);
        List<GamePkgVersionEntry> expected = new List<GamePkgVersionEntry> {
            
            new GamePkgVersionEntry {

                remoteName = "GenshinImpact_Data/Managed/Metadata/global-metadata.dat",
                md5 = "b6d43e6f1838dde18ef7b98099840098",
                fileSize = 58980820L

            },
            new GamePkgVersionEntry {

                remoteName = "GenshinImpact_Data/Managed/Resources/mscorlib.dll-resources.dat",
                md5 = "21d06dbc8af6432b2b49536ed30609af",
                fileSize = 337563L

            },
            new GamePkgVersionEntry {

                remoteName = "GenshinImpact_Data/Managed/Resources/Newtonsoft.Json.dll-resources.dat",
                md5 = "ef68c753a3826e16a982d610340a3484",
                fileSize = 639L

            }

        };

        JsonSerializer serializer = new JsonSerializer();
        Assert.That(serializer.Serialize(result), Is.EqualTo(serializer.Serialize(expected)));

    }

}
