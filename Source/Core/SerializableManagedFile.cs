using Dodoco.Core.Serialization;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core {

    public abstract class SerializableManagedFile: ManagedFile {

        public IFormatSerializer Serializer { get; protected set; }

        public SerializableManagedFile(string internalName, string directory, string fileName, IFormatSerializer serializer): base(internalName, directory, fileName) {
            
            this.Serializer = serializer;

        }

    }

}