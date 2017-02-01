using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Server.Authentication
{
    public class WindowsAuthenticationMiddleware
    {
        private RequestDelegate _next;
        private ILogger<WindowsAuthenticationMiddleware> _logger;
        private static readonly long _ntlmTokenHeader = BitConverter.ToInt64(System.Text.Encoding.ASCII.GetBytes("NTLMSSP\0"),0);
        private static readonly SessionCache _cache = new SessionCache();

        public WindowsAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<WindowsAuthenticationMiddleware>();
        }

        public Task Invoke(HttpContext httpContext)
        {
            var authorizationHeader = httpContext.Request.Headers["Authorization"];
            var hasNtlm = authorizationHeader.Any(h => h.StartsWith("NTLM "));
            if(!hasNtlm)
            {
                httpContext.Response.Headers.Add("WWW-Authenticate", new[] { "NTLM" });
                httpContext.Response.StatusCode = 401;
                return Task.FromResult(0);
            }
            var header = authorizationHeader.First(h => h.StartsWith("NTLM "));
            var token = Convert.FromBase64String(header.Substring("NTLM ".Length));
            var messageType = token[8];
            switch(messageType)
            {
                case 1:
                    //Clients first response to the challenge

                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return _next(httpContext);
        }
    }
}
