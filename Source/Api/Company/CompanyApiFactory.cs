using Dodoco.Api.Company.Launcher.Resource;
using Dodoco.Util.Log;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dodoco.Api.Company {

    public class CompanyApiFactory {

        private static CompanyApiFactory? instance = null;
        private string apiBaseUrl;
        private string key;
        private int launcherId;

        public CompanyApiFactory(string apiBaseUrl, string key, int launcherId) {

            this.apiBaseUrl = apiBaseUrl;
            this.key = key;
            this.launcherId = launcherId;

        }

        public async Task<Resource> FetchLauncherResource() {

            Logger.GetInstance().Log("Trying to fetch latest game's launcher's resource data from remote servers...");

            Resource res = new Resource();
            string urlToFetch = $"{this.apiBaseUrl}/resource?key={this.key}&launcher_id={this.launcherId}";

            HttpResponseMessage response = await Application.Application.GetInstance().client.GetAsync(urlToFetch);
        
            if (response.IsSuccessStatusCode) {

                Logger.GetInstance().Log("Successfully fetch latest game's launcher's resource data from remote servers");

                Logger.GetInstance().Log("Trying to parse the received data...");
                try { res = JsonSerializer.Deserialize<Resource>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions() {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                }); }
                catch (JsonException e) { Logger.GetInstance().Error("Failed to parse latest game's launcher's resource data. Maybe the API has been changed.", e); }
                Logger.GetInstance().Log("Successfully parsed the received data");

            } else {

                Logger.GetInstance().Error($"Failed to fetch latest game's launcher's resource data from remote servers (received HTTP status code {response.StatusCode})");

            }

            return res;

        }

    }

}