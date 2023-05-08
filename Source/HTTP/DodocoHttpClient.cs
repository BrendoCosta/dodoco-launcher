using Dodoco.Util.Log;

namespace Dodoco.HTTP {

    public class DodocoHttpClient: HttpClient {

        public async Task<HttpResponseMessage> FetchAsync(string requestUri) {

            Logger.GetInstance().Debug($"================= HTTP GET BEGIN =================");
            Logger.GetInstance().Debug($"REQUEST URI: {requestUri}");

            HttpResponseMessage res = await base.GetAsync(requestUri);

            Logger.GetInstance().Debug($"RESPONSE STATUS CODE: {res.StatusCode.ToString()}");
            Logger.GetInstance().Debug($"RESPONSE CONTENT ENCODING: {res.Content.Headers.ContentEncoding}");
            Logger.GetInstance().Debug($"RESPONSE CONTENT LENGTH: {res.Content.Headers.ContentLength}");
            Logger.GetInstance().Debug($"RESPONSE CONTENT TYPE: {res.Content.Headers.ContentType}");
            Logger.GetInstance().Debug(await res.Content.ReadAsStringAsync());
            Logger.GetInstance().Debug($"=================  HTTP GET END  =================");
            return res;

        }

    }

}