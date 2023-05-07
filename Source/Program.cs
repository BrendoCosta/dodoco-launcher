using PhotinoNET;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dodoco.Api.Company.Launcher.Resource;

namespace Dodoco {

    class Program {

        [STAThread]
        static void Main(string[] args)
        {

            Dodoco.Application.Application app = Dodoco.Application.Application.GetInstance();
            Dodoco.Launcher.Launcher launcher = Dodoco.Launcher.Launcher.GetInstance();
            
            launcher.Open();

            while (launcher.IsRunning()) {

                Thread.Sleep(500);

            }

            app.End(0);

        }

    }
    
}
