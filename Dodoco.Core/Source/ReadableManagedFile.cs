namespace Dodoco.Core {

    public abstract class ReadableManagedFile<T>: ManagedFile {

        public ReadableManagedFile(string internalName, string directory, string fileName): base(
            internalName,
            directory,
            fileName
        ) {}

        public abstract T Read();

    }

}