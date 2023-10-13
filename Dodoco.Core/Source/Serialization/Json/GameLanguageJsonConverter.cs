namespace Dodoco.Core.Serialization.Json;

using Dodoco.Core.Game;

using System.Text.Json;
using System.Text.Json.Serialization;

public class GameLanguageJsonConverter: JsonConverter<GameLanguage> {

    public override GameLanguage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        string value = reader.GetString() ?? throw new SerializationException("Null value");
        return GameConstants.SUPPORTED_VOICE_LANGUAGES.Find(someLanguage => someLanguage.Code.ToUpper() == value.ToUpper());

    }

    public override void Write(Utf8JsonWriter writer, GameLanguage value, JsonSerializerOptions options) {

        writer.WriteStringValue(value.Code);

    }

}