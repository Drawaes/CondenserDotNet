using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server.Websockets
{
    public class WebsocketMiddleware
    {
        private RequestDelegate _next;
        private ILogger _logger;

        public WebsocketMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<WebsocketMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            var upgradeFeature = context.Features.Get<IHttpUpgradeFeature>();
            context.Request.Headers.Add("TextProxyHeader", new Microsoft.Extensions.Primitives.StringValues("Test"));
            if (upgradeFeature != null && context.Features.Get< IHttpWebSocketFeature>() == null)
            {
                await DoWebSocket(context);
            }
            await _next.Invoke(context);
        }

        private async Task DoWebSocket(HttpContext context)
        {
            var service = context.Features.Get<IService>();
            var endPoint = service.IpEndPoint;
            //System.IO.Pipes.Net
        }
    }
}
