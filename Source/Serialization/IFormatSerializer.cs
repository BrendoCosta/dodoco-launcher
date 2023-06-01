namespace Dodoco.Serialization {

    public interface IFormatSerializer {

        string Serialize(object value);
        T Deserialize<T>(string value);

    }

}