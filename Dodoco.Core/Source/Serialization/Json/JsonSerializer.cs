using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Core.Serialization.Json {

    public class JsonSerializer: IFormatSerializer {

        JsonSerializerOptions options = new JsonSerializerOptions {

            NumberHandling = JsonNumberHandling.AllowReadingFromString

        };

        public JsonSerializer() {

            this.options.Converters.Add(new CultureInfoJsonConverter());
            this.options.Converters.Add(new GameLanguageJsonConverter());
            this.options.Converters.Add(new JsonStringEnumConverter());

        }

        public string Serialize(object value) {

            try {

                return System.Text.Json.JsonSerializer.Serialize(value);

            } catch (Exception e) {

                throw new SerializationException("Unable to deserialize", e);

            }

        }

        public T Deserialize<T>(string value) {

            try {

                T? data = System.Text.Json.JsonSerializer.Deserialize<T>(value, options);

                if (data != null) {

                    return data;

                } else {

                    throw new SerializationException("Unable to deserialize");

                }

            } catch (Exception e) {

                throw new SerializationException("Unable to deserialize", e);

            }

        }

    }

}