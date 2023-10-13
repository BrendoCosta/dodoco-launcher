namespace Dodoco.Core.Serialization.Yaml {

    public class YamlSerializer: IFormatSerializer {

        public string Serialize(object value) {

            YamlDotNet.Serialization.ISerializer ser = new YamlDotNet.Serialization.SerializerBuilder()
                .WithTypeConverter(new CultureInfoYamlConverter())
                .WithTypeConverter(new GameLanguageYamlConverter())
                .WithTypeConverter(new DirectoryInfoYamlConverter())
                .WithTypeConverter(new FileInfoYamlConverter())
                .Build();

            return ser.Serialize(value);

        }

        public T Deserialize<T>(string value) {

            YamlDotNet.Serialization.IDeserializer dsr = new YamlDotNet.Serialization.DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithTypeConverter(new CultureInfoYamlConverter())
                .WithTypeConverter(new GameLanguageYamlConverter())
                .WithTypeConverter(new DirectoryInfoYamlConverter())
                .WithTypeConverter(new FileInfoYamlConverter())
                .Build();

            return dsr.Deserialize<T>(value);

        }

    }

}