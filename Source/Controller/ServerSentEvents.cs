namespace Dodoco.Controller {

    [Grapevine.RestResource(BasePath = "/Dodoco/SSE")]
    public static class ServerSentEvents {

        private static Dictionary<string, Func<Dodoco.HTTP.SSE.Event>> events = new Dictionary<string, Func<Dodoco.HTTP.SSE.Event>>();

        public static void RegisterEvent(string key, Func<Dodoco.HTTP.SSE.Event> function) => events.Add(key, function);
        public static void UnregisterEvent(string key) => events.Remove(key);

        [Grapevine.RestRoute("Get", "/")]
        public static async Task Root(Grapevine.IHttpContext context) {

            Dodoco.HTTP.SSE.HTTPContext ctx = new Dodoco.HTTP.SSE.HTTPContext(context);

            await Task.Run(async () => {

                while (true) {

                    foreach (KeyValuePair<string, Func<HTTP.SSE.Event>> entry in events) {

                        Dodoco.HTTP.SSE.Event _event = entry.Value.Invoke();
                        await ctx.WriteEventToResponseOutputStream(_event);

                    }

                    await Task.Delay(500);

                }

            });

            ctx.CloseResponse();
            
        }

    }

}