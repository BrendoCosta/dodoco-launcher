using Dodoco.Application;
using Dodoco.Game;
using Dodoco.Launcher.Cache;
using Dodoco.Launcher.Settings;
using Dodoco.Network.Api.Company.Launcher.Content;
using Dodoco.Network.Api.Company.Launcher.Resource;
using System.ComponentModel;

namespace Dodoco.Launcher {

    public interface ILauncher {

        //void Start();
        //void Stop();
        bool IsRunning();
        
        //Task StartGame();
        //Task CloseGame();
        Task RepairGameFiles();
        //Task DownloadGame();
        //Task UpdateGame();
        //Task<List<GameIntegrityReport>> CheckGameIntegrity();
        //Task<int> DownloadGameOperationStatus();
        //Task<int> UpdateGameOperationStatus();

        event EventHandler<IGame> OnGameCreated;

        void SetLauncherCache(LauncherCache cache);
        void SetLauncherSettings(LauncherSettings settings);

        LauncherActivityState GetLauncherActivityState();
        LauncherCache GetLauncherCache();
        Content GetContent();
        LauncherExecutionState GetLauncherExecutionState();
        IGame GetGame();
        Resource GetResource();
        LauncherSettings GetLauncherSettings();

    }

}