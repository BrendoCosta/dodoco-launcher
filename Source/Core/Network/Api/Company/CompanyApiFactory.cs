using Dodoco.Core.Network.Api.Company.Launcher.Content;
using Dodoco.Core.Network.Api.Company.Launcher.Resource;
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
        private CultureInfo language;

        public CompanyApiFactory(string apiBaseUrl, string key, int launcherId, CultureInfo language) {

            this.apiBaseUrl = apiBaseUrl;
            this.key = key;
            this.launcherId = launcherId;
            this.language = language;

        }

        public async Task<Content> FetchLauncherContent() {

            Logger.GetInstance().Log("Fetching latest game's launcher's content data from remote servers...");
            Content? content = await this.FetchApi<Content>($"/content?key={this.key}&launcher_id={this.launcherId}&language={this.language.Name.ToLower()}");
            if (content == null)
                throw new NetworkException("Failed to fetch content API");
            return content;

        }

        public async Task<Resource> FetchLauncherResource() {

            Logger.GetInstance().Log("Fetching latest game's launcher's resource data from remote servers...");
            Resource? resource = await this.FetchApi<Resource>($"/resource?key={this.key}&launcher_id={this.launcherId}");
            if (resource == null)
                throw new NetworkException("Failed to fetch resource API");
            return resource;

        }

        private async Task<T?> FetchApi<T>(string apiUri) where T: CompanyApi {

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