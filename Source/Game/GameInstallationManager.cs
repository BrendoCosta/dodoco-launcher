using Dodoco.Util.Log;
using System.Text;
using System.Text.RegularExpressions;

namespace Dodoco.Game {

    public static class GameInstallationManager {

        public static bool CheckGameInstallation(string gameInstallationDirectory, GameServer gameServer) {

            Logger.GetInstance().Log($"Checking game installation in ({gameInstallationDirectory})...");
            string gameExecutableFileName = $"{GameConstants.GAME_TITLE[gameServer]}.exe";
            string gameExecutablePath = Path.Join(gameInstallationDirectory, gameExecutableFileName);

            if (Directory.Exists(gameInstallationDirectory)) {

                if (File.Exists(gameExecutablePath)) {

                    Logger.GetInstance().Log($"Successfully found game's executable ({gameExecutableFileName}) inside the specified directory ({gameInstallationDirectory})");
                    return true;

                } else {

                    Logger.GetInstance().Warning($"Unable to find game's executable ({gameExecutableFileName}) inside the specified directory");

                }

            } else {

                Logger.GetInstance().Warning($"The specified game installation directory ({gameInstallationDirectory}) doesn't exists");

            }

            Logger.GetInstance().Warning($"Game not found in specified directory");

            return false;

        }

        public static Version SearchForGameVersion(string gameInstallationDirectory, GameServer gameServer) {

            string dataDirectory = Path.Join(gameInstallationDirectory, $"{GameConstants.GAME_TITLE[gameServer]}_Data");
            string gameManagerFileName = "globalgamemanagers";
            string gameManagerFilePath = Path.Join(dataDirectory, gameManagerFileName);

            Logger.GetInstance().Log($"Trying to find game's data directory ({dataDirectory}) exists...");
            
            if (Directory.Exists(dataDirectory)) {

                Logger.GetInstance().Log($"Successfully found game's data directory");
                Logger.GetInstance().Log($"Trying to find \"{gameManagerFileName}\" file inside data directory...");

                if (File.Exists(gameManagerFilePath)) {

                    Logger.GetInstance().Log($"Successfully found \"{gameManagerFileName}\" file inside data directory");
                    Logger.GetInstance().Log($"Trying to read \"{gameManagerFileName}\" file from data directory...");

                    try {

                        byte[] buffer = File.ReadAllBytes(gameManagerFilePath);

                        if (buffer.Length == 0) {

                            throw new GameException($"The file \"{gameManagerFileName}\" ({gameManagerFilePath}) is empty");

                        }

                        Logger.GetInstance().Log($"Successfully read \"{gameManagerFileName}\" file from data directory");
                        Logger.GetInstance().Log($"Trying to find the game version inside the \"{gameManagerFileName}\" file...");

                        string fileContents = Encoding.ASCII.GetString(buffer);
                        
                        foreach (Match match in Regex.Matches(fileContents, @"([1-9]+\.[0-9]+\.[0-9]+)_[\d]+_[\d]+")) {

                            Version version = Version.Parse(match.ToString().Split("_")[0]);
                            Logger.GetInstance().Log($"Successfully found game version ({version}) inside the \"{gameManagerFileName}\" file");
                            return version;

                        }

                        throw new GameException($"Can't find the game version inside the \"{gameManagerFileName}\" file");

                    } catch (Exception e) {

                        throw new GameException($"Failed to read the file \"{gameManagerFileName}\" ({gameManagerFilePath})", e);

                    }

                } else {

                    throw new GameException($"The file \"{gameManagerFileName}\" is missing");

                }

            } else {

                throw new GameException($"Game's data directory ({dataDirectory}) doesn't exists");

            }

        }

    }

}