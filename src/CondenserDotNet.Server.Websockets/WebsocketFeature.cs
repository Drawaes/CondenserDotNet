using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace CondenserDotNet.Server.Websockets
{
    public class WebsocketFeature : IHttpWebSocketFeature
    {
        private HttpContext _context;
        IHttpUpgradeFeature _upgrade;

        public WebsocketFeature(HttpContext context, IHttpUpgradeFeature upgradeFeature)
        {
            _upgrade = upgradeFeature;
            _context = context;
        }

        public bool IsWebSocketRequest
        {
            get
            {
                return true;
            }
        }

        public Task<WebSocket> AcceptAsync(WebSocketAcceptContext context)
        {
            throw new NotImplementedException();
        }
    }
}
