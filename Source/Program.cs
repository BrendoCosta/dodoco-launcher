using PhotinoNET;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco {

    class Program {

        [STAThread]
        static void Main(string[] args)
        {

            Dodoco.Application.Application app = Dodoco.Application.Application.GetInstance();
            Dodoco.Launcher.Launcher launcher = Dodoco.Launcher.Launcher.GetInstance();
            
            launcher.Run();

            while (launcher.IsRunning()) {

                Thread.Sleep(500);

            }

            app.End(0);

        }

    }
    
}
