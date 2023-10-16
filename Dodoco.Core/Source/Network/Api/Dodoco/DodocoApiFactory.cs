using Dodoco.Core.Game;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Network.Api.Github.Repos.Content;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Util.Log;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Core.Network.Api.Dodoco {

    public class DodocoApiFactory {

        public Uri BaseUrl { get; private set; }
        private JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public DodocoApiFactory(string baseUrl) {

            this.BaseUrl = new Uri(baseUrl);

        }

        public async Task<ResourceResponse> FetchCachedLauncherResource(Version gameVersion, GameServer gameServer) {

            Logger.GetInstance().Log("Fetching cached launcher resource data from remote servers...");
            List<Content>? contentList = await this.FetchApi<List<Content>>("contents/Api/Cached/Company/Launcher/Resource");

            if (contentList != null) {

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

                                ResourceResponse? res = JsonSerializer.Deserialize<ResourceResponse>(Convert.FromBase64String(await response.Content.ReadAsStringAsync()), jsonOptions);
                                
                                if (res != null) {

                                    Logger.GetInstance().Log("Successfully parsed the cached launcher resource file's data");
                                    return res;

                                }

                            } catch (JsonException e) {
                                
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

        private async Task<T?> FetchApi<T>(string resourceUrl) {

            T? data = Activator.CreateInstance<T>();
            Uri urlToFetch = new Uri(this.BaseUrl, resourceUrl);
            
            HttpResponseMessage response = await Client.GetInstance().FetchAsync(urlToFetch);
            
            if (response.IsSuccessStatusCode) {

                Logger.GetInstance().Log("Successfully fetch latest data from remote servers");

                Logger.GetInstance().Log("Trying to parse the received data...");
                try { data = JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions() {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                }); }
                catch (JsonException e) { throw new NetworkException("Failed to parse the received. Maybe the API has been changed?", e); }
                Logger.GetInstance().Log("Successfully parsed the received data");

            } else {

                throw new NetworkException($"Failed to fetch latest data from remote servers (received HTTP status code {response.StatusCode})");

            }

            return data;
            
        }

    }

}