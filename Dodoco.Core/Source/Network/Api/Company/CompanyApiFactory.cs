using Dodoco.Core.Game;
using Dodoco.Core.Protocol.Company.Launcher;
using Dodoco.Core.Protocol.Company.Launcher.Content;
using Dodoco.Core.Protocol.Company.Launcher.Resource;
using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Util.Log;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Core.Network.Api.Company {

    public class CompanyApiFactory {

        private string apiBaseUrl;
        private string key;
        private int launcherId;
        private GameLanguage language;
        private static Dictionary<Tuple<string, int>, Cache<ResourceResponse>> ResourceResponseCache = new Dictionary<Tuple<string, int>, Cache<ResourceResponse>>();
        private Tuple<string, int> cacheKey { get => new Tuple<string, int>(this.key, this.launcherId); }

        public CompanyApiFactory(string apiBaseUrl, string key, int launcherId, GameLanguage language) {

            this.apiBaseUrl = apiBaseUrl;
            this.key = key;
            this.launcherId = launcherId;
            this.language = language;

        }

        public virtual async Task<ContentResponse> FetchLauncherContent() {

            Logger.GetInstance().Log("Fetching latest game's launcher's content data from remote servers...");
            ContentResponse? content = await this.FetchApi<ContentResponse>($"/content?key={this.key}&launcher_id={this.launcherId}&language={this.language.Name.ToLower()}");
            if (content == null)
                throw new NetworkException("Failed to fetch content API");
            return content;

        }

        public virtual async Task<ResourceResponse> FetchLauncherResource() {

            if (!ResourceResponseCache.ContainsKey(this.cacheKey)) {

                ResourceResponseCache.Add(cacheKey, new Cache<ResourceResponse>(new ResourceResponse()));

            }

            if (ResourceResponseCache[this.cacheKey].IsValid()) {

                return ResourceResponseCache[this.cacheKey].Resource;

            }

            Logger.GetInstance().Log("Fetching latest game's launcher's resource data from remote servers...");
            ResourceResponse? resource = await this.FetchApi<ResourceResponse>($"/resource?key={this.key}&launcher_id={this.launcherId}");
            if (resource == null)
                throw new NetworkException("Failed to fetch resource API");

            ResourceResponseCache[this.cacheKey].Update(resource);
            return resource;

        }

        private async Task<T?> FetchApi<T>(string apiUri) where T: LauncherResponse {

            T? data = Activator.CreateInstance<T>();
            string urlToFetch = this.apiBaseUrl + apiUri;
            HttpResponseMessage response = await Client.GetInstance().FetchAsync(new Uri(urlToFetch));

            if (response.IsSuccessStatusCode) {

                Logger.GetInstance().Log("Successfully fetch latest data from remote servers");

                Logger.GetInstance().Log("Trying to parse the received data...");
                try { data = JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions() {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                }); }
                catch (JsonException e) { throw new NetworkException("Failed to parse the received data. Maybe the API has been changed?", e); }
                Logger.GetInstance().Log("Successfully parsed the received data");

            } else {

                throw new NetworkException($"Failed to fetch latest data from remote servers (received HTTP status code {response.StatusCode})");

            }

            return data;
            
        }

    }

}