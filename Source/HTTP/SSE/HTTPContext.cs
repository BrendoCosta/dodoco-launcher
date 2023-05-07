namespace Dodoco.HTTP.SSE {

    public class HTTPContext {

        public System.Net.HttpListenerContext? context;
        public Grapevine.IHttpContext saveCtx;

        public HTTPContext(Grapevine.IHttpContext context) {

            this.saveCtx = context;

            Grapevine.HttpContext? ctx = context as Grapevine.HttpContext;

            if (ctx != null) {

                this.context = ctx.Advanced;
                this.context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                this.context.Response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
                this.context.Response.Headers["Cache-Control"] = "no-cache";
                this.context.Response.Headers["Connection"] = "keep-alive";

            }

        }

        public async Task WriteEventToResponseOutputStream(Dodoco.HTTP.SSE.Event _event) {

            if (this.context != null) {

                byte[] txt = Dococo.Util.Text.StringUtil.UTF8StringToUTF8Bytes(_event.ToString());

                await this.context.Response.OutputStream.WriteAsync(txt, 0, txt.Length);

            }

        }

        public void CloseResponse() {

            if (this.context != null) {

                this.context.Response.OutputStream.Close();
                this.context.Response.Close();

            }

        }

    }

}