using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using System.Globalization;

namespace Dodoco.Serialization.Yaml {

    public class CultureInfoYamlConverter: IYamlTypeConverter {

        public bool Accepts(Type type) => type == typeof(CultureInfo);

        public object? ReadYaml(IParser parser, Type type) {

            Scalar scalar = parser.Consume<Scalar>();
            CultureInfo culture = new CultureInfo(scalar.Value);
            return culture;

        }

        public void WriteYaml(IEmitter emitter, object? value, Type type) {

            CultureInfo? culture = (CultureInfo?) value;

            if (culture != null) {

                emitter.Emit(new Scalar(culture.Name));

            }

        }

    }

}