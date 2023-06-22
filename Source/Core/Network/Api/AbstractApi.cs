using Dodoco.Core.Network.HTTP;
using Dodoco.Core.Serialization;
using Dodoco.Core.Util.Log;

namespace Dodoco.Core.Network.Api {

    public abstract class AbstractApi {

        private IFormatSerializer FormatSerializer;

        protected AbstractApi(IFormatSerializer formatSerializer) {

            this.FormatSerializer = formatSerializer;

        }

        protected virtual async Task<T?> FetchApi<T>(string url) => await this.FetchApi<T>(new Uri(url));
        
        protected virtual async Task<HttpResponseMessage> MakeRequest(Uri url) {

            return await Client.GetInstance().FetchAsync(url);

        }

        protected virtual async Task<T?> FetchApi<T>(Uri url) {

            T? data = Activator.CreateInstance<T>();

            using (HttpResponseMessage response = await this.MakeRequest(url)) {

                if (response.IsSuccessStatusCode) {

                    Logger.GetInstance().Log("Successfully fetch API data from remote servers");
                    
                    try {

                        Logger.GetInstance().Log("Trying to parse the received API data...");
                        data = this.FormatSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
                        Logger.GetInstance().Log("Successfully parsed the received API data");
                        
                    } catch (SerializationException e) {
                        
                        throw new NetworkException("Failed to parse the received API data. Maybe the API has been changed?", e);
                        
                    }

                } else {

                    throw new NetworkException($"Failed to fetch the API data from remote servers (received HTTP status code {response.StatusCode})");

                }

            }

            return data;

        }

    }

}