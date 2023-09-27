using Dodoco.Core.Util.Log;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Dodoco.Core.Wine {

    public class Wine: IWine {

        public string Directory { get; protected set; }
        public string PrefixDirectory { get; protected set; }
        public string ExecutableFullPath { get => Path.Join(this.Directory, "/bin/wine64"); }
        public WineState State { get; protected set; } = WineState.UNINITIALIZED;
        protected string WineBinaryFilePath;

        public Wine(string directory, string prefixDirectory) {

            this.Directory = directory;
            this.PrefixDirectory = prefixDirectory;
            this.WineBinaryFilePath = Path.Join(directory, "/bin/wine");

            if (File.Exists(this.WineBinaryFilePath)) {

                this.UpdateState(WineState.READY);

            } else {

                this.UpdateState(WineState.NOT_FOUND);

            }
            
        }

        public void SetPrefixDirectory(string directory) => this.PrefixDirectory = directory;

        public async Task<string> GetWineVersion() {

            if (!File.Exists(this.WineBinaryFilePath))
                throw new WineException($"Can't find the Wine's executable in the path \"{this.WineBinaryFilePath}\"");

            Process process = new Process();

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Path.Join(this.Directory, "/bin/wine64");
            info.WorkingDirectory = Path.Join(this.Directory, "/bin/");
            info.Arguments = $"--version";
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;

            process.StartInfo = info;

            Logger.GetInstance().Log($"Starting Wine's process with command \"{info.FileName} {info.Arguments}\"...");

            try {

                if (process.Start()) {

                    Logger.GetInstance().Log($"Successfully started Wine's process inside directory \"{info.WorkingDirectory}\" with the command \"{info.FileName} {info.Arguments}\"");

                    await process.WaitForExitAsync();

                    Logger.GetInstance().Log($"Successfully finished Wine's process");
                    Logger.GetInstance().Log($"Reading Wine's process output...");

                    string? output = process.StandardOutput.ReadToEnd();

                    if (output != null) {

                        foreach (Match match in Regex.Matches(output, @"(^wine-).*")) {

                            string version = match.ToString();
                            Logger.GetInstance().Log($"Successfully found Wine's version \"{version}\" at \"{this.Directory}\" ");
                            return version;

                        }

                        throw new WineException($"Unable to get the version from Wine's process output");

                    } else {

                        throw new WineException($"Received a null Wine's process output");

                    }

                } else {

                    throw new WineException($"Failed to start the Wine's process inside directory \"{info.WorkingDirectory}\" with the command \"{info.FileName} {info.Arguments}\"");

                }

            } catch (Exception e) {

                throw new WineException($"Unable to get Wine's version", e);
                
            }

        }

        public virtual async Task Execute(string executablePath, List<string>? arguments = null) {

            Process process = new Process();
            ProcessStartInfo info = new ProcessStartInfo();

            info.FileName = this.ExecutableFullPath;
            info.WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            info.EnvironmentVariables.Add("WINEPREFIX", this.PrefixDirectory);
            info.Arguments = executablePath;
            if (arguments != null)
               info.Arguments += $" {String.Join(" ", arguments)}";
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;

            Logger.GetInstance().Debug(info.Arguments);

            process.StartInfo = info;

            process.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) => {
                
                // TODO: Select what Wine debug messages will be logged
                //if (!string.IsNullOrWhiteSpace(e.Data))
                //    Logger.GetInstance().Log($"Wine: {e.Data}");
                
            });

            process.ErrorDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) => {
                
                // TODO: Select what Wine debug messages will be logged
                //if (!string.IsNullOrWhiteSpace(e.Data))
                //    Logger.GetInstance().Log($"Wine: {e.Data}");
                
            });

            try {

                if (process.Start()) {

                    Logger.GetInstance().Log($"Successfully started Wine's process");

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    
                    await process.WaitForExitAsync();

                    Logger.GetInstance().Log($"Successfully finished Wine's process");

                }

            } catch (Exception e) {

                throw new WineException($"Error starting Wine's process", e);

            }
            
        }

        protected void UpdateState(WineState newState) {

            Logger.GetInstance().Debug($"Updating Wine state from {this.State.ToString()} to {newState.ToString()}");
            this.State = newState;

        }

    }

}