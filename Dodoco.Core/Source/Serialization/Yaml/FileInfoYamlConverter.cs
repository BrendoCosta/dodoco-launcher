using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using System.Globalization;

namespace Dodoco.Core.Serialization.Yaml {

    public class FileInfoYamlConverter: IYamlTypeConverter {

        public bool Accepts(Type type) => type == typeof(FileInfo);

        public object? ReadYaml(IParser parser, Type type) {

            Scalar scalar = parser.Consume<Scalar>();
            FileInfo directory = new FileInfo(!string.IsNullOrWhiteSpace(scalar.Value) ? scalar.Value : "./");
            return directory;

        }

        public void WriteYaml(IEmitter emitter, object? value, Type type) {

            FileInfo? directory = (FileInfo?) value;

            if (directory != null) {

                emitter.Emit(new Scalar(directory.FullName));

            }

        }

    }

}