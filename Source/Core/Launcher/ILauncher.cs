using Dodoco.Core.Game;
using Dodoco.Core.Launcher.Cache;
using Dodoco.Core.Launcher.Settings;
using Dodoco.Core.Network.Api.Company.Launcher.Content;
using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Wine;

namespace Dodoco.Core.Launcher {

    public interface ILauncher {

        LauncherState State { get; }

        event EventHandler BeforeStart;
        event EventHandler AfterStart;
        event EventHandler<IGame> OnGameCreated;

        Task Start();
        Task Stop();
        
        //Task StartGame();
        //Task CloseGame();
        //Task DownloadGame();
        //Task UpdateGame();
        //Task<List<GameIntegrityReport>> CheckGameIntegrity();
        //Task<int> DownloadGameOperationStatus();
        //Task<int> UpdateGameOperationStatus();

        void UpdateLauncherCache(LauncherCache cache);
        void UpdateLauncherSettings(LauncherSettings settings);

        LauncherCache GetLauncherCache();
        Content GetContent();
        LauncherExecutionState GetLauncherExecutionState();
        IGame? GetGame();
        Resource GetResource();
        LauncherSettings GetLauncherSettings();
        IWine GetWine();

    }

}