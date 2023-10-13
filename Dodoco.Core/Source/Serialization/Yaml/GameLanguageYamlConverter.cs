namespace Dodoco.Core.Serialization.Yaml;

using Dodoco.Core.Game;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public class GameLanguageYamlConverter: IYamlTypeConverter {

    public bool Accepts(Type type) => type == typeof(GameLanguage);

    public object? ReadYaml(IParser parser, Type type) {

        Scalar scalar = parser.Consume<Scalar>();
        return GameConstants.SUPPORTED_VOICE_LANGUAGES.Find(someLanguage => someLanguage.Code.ToUpper() == scalar.Value.ToUpper());

    }

    public void WriteYaml(IEmitter emitter, object? value, Type type) {

        if (value == null)
            return;

        emitter.Emit(new Scalar(((GameLanguage) value).Code));

    }

}