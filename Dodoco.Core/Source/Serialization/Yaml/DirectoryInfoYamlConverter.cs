using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using System.Globalization;

namespace Dodoco.Core.Serialization.Yaml {

    public class DirectoryInfoYamlConverter: IYamlTypeConverter {

        public bool Accepts(Type type) => type == typeof(DirectoryInfo);

        public object? ReadYaml(IParser parser, Type type) {

            Scalar scalar = parser.Consume<Scalar>();
            DirectoryInfo directory = new DirectoryInfo(!string.IsNullOrWhiteSpace(scalar.Value) ? scalar.Value : "./");
            return directory;

        }

        public void WriteYaml(IEmitter emitter, object? value, Type type) {

            DirectoryInfo? directory = (DirectoryInfo?) value;

            if (directory != null) {

                emitter.Emit(new Scalar(directory.FullName));

            }

        }

    }

}