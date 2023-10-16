namespace Dodoco.Core.Game;

using Dodoco.Core.Serialization.Json;

public class PkgVersionParser {

    public static List<GamePkgVersionEntry> Parse(string content) {

        List<GamePkgVersionEntry> entries = new List<GamePkgVersionEntry>();

        using (StringReader reader = new StringReader(content)) {

            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine()) {

                GamePkgVersionEntry entry = new JsonSerializer().Deserialize<GamePkgVersionEntry>(line);
                entries.Add(entry);

            }

        }

        return entries;

    }

}