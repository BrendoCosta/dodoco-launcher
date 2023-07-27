namespace Dodoco.Core.Wine {

    public interface IWine {

        string Directory { get; }
        string PrefixDirectory { get; }
        WineState State { get; }

        void SetPrefixDirectory(string directory);
        Task<string> GetWineVersion();
        Task Execute(string executablePath, List<string>? arguments = null);

    }

}