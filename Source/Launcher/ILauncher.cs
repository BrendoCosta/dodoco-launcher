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

        //Task CheckGameFilesIntegrity();
        //Task DownloadGame();
        //Task UpdateGame();
        //Task<List<GameIntegrityReport>> CheckGameIntegrity();
        //Task<int> DownloadGameOperationStatus();
        //Task<int> UpdateGameOperationStatus();

        event EventHandler<int> OnOperationProgressChanged;

        LauncherActivityState ActivityState { get; }
        LauncherCache Cache { get; }
        Content Content { get; }
        LauncherExecutionState ExecutionState { get; }
        IGame? Game { get; }
        Resource Resource { get; }
        LauncherSettings Settings { get; }

        LauncherActivityState GetLauncherActivityState();
        LauncherCache GetLauncherCache();
        Content GetContent();
        LauncherExecutionState GetLauncherExecutionState();
        IGame GetGame();
        Resource GetResource();
        LauncherSettings GetLauncherSettings();

        Task RepairGameFiles();

        void SetLauncherCache(LauncherCache cache);
        void SetLauncherSettings(LauncherSettings settings);

    }

}