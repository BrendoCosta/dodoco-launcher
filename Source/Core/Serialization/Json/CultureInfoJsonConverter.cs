using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Core.Serialization.Json {

    public class CultureInfoJsonConverter: JsonConverter<CultureInfo> {

        public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            return new CultureInfo(reader.GetString()!);

        }

        public override void Write(Utf8JsonWriter writer, CultureInfo cultureInfoValue, JsonSerializerOptions options) {

            writer.WriteStringValue(cultureInfoValue.Name);

        }

    }

}