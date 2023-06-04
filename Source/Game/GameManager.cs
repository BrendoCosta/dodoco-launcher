using Dodoco.Network.Api.Company.Launcher.Resource;
using Dodoco.Util.Log;

using System.Text;
using System.Text.RegularExpressions;

namespace Dodoco.Game {

    public static class GameManager {

        public static bool CheckGameInstallation(string gameInstallationDirectory, GameServer gameServer) {

            Logger.GetInstance().Log($"Checking game installation in ({gameInstallationDirectory})...");
            string gameExecutableFileName = $"{GameConstants.GAME_TITLE[gameServer]}.exe";
            FileInfo gameExecutable = new FileInfo(Path.Join(gameInstallationDirectory, gameExecutableFileName));

            if (Directory.Exists(gameInstallationDirectory)) {

                if (gameExecutable.Exists) {

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

            DirectoryInfo dataDirectory = new DirectoryInfo(Path.Join(gameInstallationDirectory, $"{GameConstants.GAME_TITLE[gameServer]}_Data"));
            FileInfo globalGameManagerFile = new FileInfo(Path.Join(dataDirectory.FullName, "globalgamemanagers"));

            Logger.GetInstance().Log($"Trying to find game's data directory ({dataDirectory.FullName})...");
            
            if (dataDirectory.Exists) {

                Logger.GetInstance().Log($"Successfully found game's data directory");
                Logger.GetInstance().Log($"Trying to find \"{globalGameManagerFile.Name}\" file inside data directory...");

                if (globalGameManagerFile.Exists) {

                    Logger.GetInstance().Log($"Successfully found \"{globalGameManagerFile.Name}\" file inside data directory");
                    Logger.GetInstance().Log($"Trying to read \"{globalGameManagerFile.Name}\" file from data directory...");

                    try {

                        byte[] buffer = File.ReadAllBytes(globalGameManagerFile.FullName);

                        if (buffer.Length == 0) {

                            throw new GameException($"The file \"{globalGameManagerFile.Name}\" ({globalGameManagerFile.FullName}) is empty");

                        }

                        Logger.GetInstance().Log($"Successfully read \"{globalGameManagerFile.Name}\" file from data directory");
                        Logger.GetInstance().Log($"Trying to find the game version inside the \"{globalGameManagerFile.Name}\" file...");

                        string fileContents = Encoding.ASCII.GetString(buffer);
                        
                        foreach (Match match in Regex.Matches(fileContents, @"([1-9]+\.[0-9]+\.[0-9]+)_[\d]+_[\d]+")) {

                            Version version = Version.Parse(match.ToString().Split("_")[0]);
                            Logger.GetInstance().Log($"Successfully found game version ({version}) inside the \"{globalGameManagerFile.Name}\" file");
                            return version;

                        }

                        throw new GameException($"Can't find the game version inside the \"{globalGameManagerFile.Name}\" file");

                    } catch (Exception e) {

                        throw new GameException($"Failed to read the file \"{globalGameManagerFile.Name}\" ({globalGameManagerFile.FullName})", e);

                    }

                } else {

                    throw new GameException($"The file \"{globalGameManagerFile.Name}\" is missing");

                }

            } else {

                throw new GameException($"Game's data directory ({dataDirectory.FullName}) doesn't exists");

            }

        }

        public static IMutableGame CreateGame(Version version, GameServer server, Resource resource, string directory, GameState state) {

            Logger.GetInstance().Log($"Creating game instance...");
            
            IMutableGame stable = new GameStable(version, server, resource, directory, state);

            try {

                return new Dictionary<Version, IMutableGame> {

                    //{ Version.Parse("3.7.0"), stable },
                    //{ Version.Parse("3.6.0"), stable }

                }[version];

            } catch (KeyNotFoundException) {

                Logger.GetInstance().Warning($"There is no game interface for the given version ({version.ToString()}). The current stable interface will be used instead, but be aware that unknown errors may occur. Newer launcher updates may add new game interfaces who support the given version.");
                return stable;

            }

        }

    }

}