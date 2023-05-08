using Dodoco.Api.Company.Launcher;
using Dodoco.Util.Log;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Api.Company {

    public class CompanyApiFactory {

        private static CompanyApiFactory? instance = null;
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

        public async Task<Content?> FetchLauncherContent() {

            Logger.GetInstance().Log("Fetching latest game's launcher's content data from remote servers...");
            Content? content = await this.FetchApi<Content>($"/content?key={this.key}&launcher_id={this.launcherId}&language={this.language.Name.ToLower()}");
            return content;

        }

        public async Task<Resource?> FetchLauncherResource() {

            Logger.GetInstance().Log("Fetching latest game's launcher's resource data from remote servers...");
            Resource? res = await this.FetchApi<Resource>($"/resource?key={this.key}&launcher_id={this.launcherId}");
            return res;

        }

        private async Task<CompanyApi?> FetchApi<CompanyApi>(string apiUri) {

            CompanyApi? data = default(CompanyApi);
            string urlToFetch = this.apiBaseUrl + apiUri;
            HttpResponseMessage response = await Application.Application.GetInstance().client.FetchAsync(urlToFetch);
        
            if (response.IsSuccessStatusCode) {

                Logger.GetInstance().Log("Successfully fetch latest data from remote servers");

                Logger.GetInstance().Log("Trying to parse the received data...");
                try { data = JsonSerializer.Deserialize<CompanyApi>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions() {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                }); }
                catch (JsonException e) { Logger.GetInstance().Error("Failed to parse the received. Maybe the API has been changed?", e); }
                Logger.GetInstance().Log("Successfully parsed the received data");

            } else {

                Logger.GetInstance().Error($"Failed to fetch latest data from remote servers (received HTTP status code {response.StatusCode})");

            }

            return data;
            
        }

    }

}