using Dodoco.Core.Game;
using Dodoco.Core.Network.Api.Company.Launcher.Resource;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Serialization;
using Dodoco.Core.Serialization.Json;
using Dodoco.Core.Util.Log;

using System.Text;

namespace Dodoco.Core.Network.Api.Github.Repos {

    public class LauncherRepositoryApi: GitHubReposApiFactory {

        public LauncherRepositoryApi(GitHubReposApiConfig config): base(config) {}

        public async Task<Resource> FetchCachedLauncherResource(Version gameVersion, GameServer gameServer) {

            Logger.GetInstance().Log("Fetching cached launcher resource data from remote servers...");
            List<Content.Content>? contentList = await this.FetchContents("Api/Cached/Company/Launcher/Resource");

            if (contentList != null) {

                if (contentList.Count == 0)
                    throw new NetworkException("Empty content list");

                string fileNameToSearch = $"{gameVersion.ToString()}.{gameServer.ToString()}";
                Logger.GetInstance().Log($"Searching for the cached launcher resource file \"{fileNameToSearch}\" in remote content...");

                foreach (var content in contentList) {

                    if (content.name == fileNameToSearch) {

                        Logger.GetInstance().Log($"Successfully found the cached launcher resource file in remote content");
                        Logger.GetInstance().Log($"Fetching the cached launcher resource file...");

                        HttpResponseMessage response = await Client.GetInstance().FetchAsync(content.download_url);

                        if (response.IsSuccessStatusCode) {

                            Logger.GetInstance().Log($"Successfully fetched the cached launcher resource file from remote servers");

                            try {

                                Logger.GetInstance().Log($"Trying to parse the cached launcher resource file's data");

                                Resource? res = new JsonSerializer().Deserialize<Resource>(Encoding.UTF8.GetString(Convert.FromBase64String(await response.Content.ReadAsStringAsync())));
                                
                                if (res != null) {

                                    Logger.GetInstance().Log("Successfully parsed the cached launcher resource file's data");
                                    return res;

                                }

                            } catch (SerializationException e) {
                                
                                throw new NetworkException("Failed to parse the cached launcher resource file. Maybe the API has been changed?", e);
                                
                            }

                        } else {

                            throw new NetworkException($"Failed to fetch the cached launcher resource file from remote servers (received HTTP status code {response.StatusCode})");

                        }

                    }

                }

                throw new NetworkException($"Can't find the requested cached launcher resource data among fetched files");

            }

            throw new NetworkException("Failed to fetch cached launcher resource data");

        }

    }

}