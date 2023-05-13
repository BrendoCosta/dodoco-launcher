using  Dodoco.HTTP.SSE;

using System.Collections;

namespace Dodoco.Controller {

    [Grapevine.RestResource(BasePath = "/Dodoco/SSE")]
    public static class ServerSentEvents {

        private static Queue eventQueue = Queue.Synchronized(new Queue());
        private static Dictionary<string, Func<Event>> events = new Dictionary<string, Func<Event>>();

        public static void RegisterEvent(string key, Func<Event> function) => events.Add(key, function);
        public static void UnregisterEvent(string key) => events.Remove(key);
        public static void PushEvent(Event e) => eventQueue.Enqueue(e);

        [Grapevine.RestRoute("Get", "/")]
        public static async Task Root(Grapevine.IHttpContext context) {

            Dodoco.HTTP.SSE.HTTPContext ctx = new Dodoco.HTTP.SSE.HTTPContext(context);

            await Task.Run(async () => {

                while (true) {

                    /*
                     * Push registered events
                    */

                    foreach (KeyValuePair<string, Func<HTTP.SSE.Event>> entry in events) {

                        Event _event = entry.Value.Invoke();
                        await ctx.WriteEventToResponseOutputStream(_event);

                    }

                    /*
                     * Push events from the queue
                    */

                    if (eventQueue.Count != 0) {

                        Event? _event = (Event?) eventQueue.Dequeue();

                        if (_event != null)
                            await ctx.WriteEventToResponseOutputStream(_event);

                    }

                    await Task.Delay(50);

                }

            });

            ctx.CloseResponse();
            
        }

    }

}