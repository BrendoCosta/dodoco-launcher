namespace Dodoco.Network.Controller {

    [Grapevine.RestResource(BasePath = "/Dodoco/LoggerController")]
    public class LoggerController {

        [Grapevine.RestRoute("Get", "/GetFullLogJson")]
        public async Task GetFullLogJson(Grapevine.IHttpContext context) {

            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = Grapevine.ContentType.Json;

            await context.Response.SendResponseAsync(
                Dodoco.Util.Text.StringUtil.UTF8StringToUTF8Bytes(
                    Dodoco.Util.Log.Logger.GetInstance().GetFullLogJson()
                )
            );
            
        }

    }

}