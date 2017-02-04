using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Server.WindowsAuthentication
{
    public class WindowsAuthenticationMiddleware
    {
        private RequestDelegate _next;
        private ILogger<WindowsAuthenticationMiddleware> _logger;
        private static readonly WindowsAuthHandshakeCache _cache = new WindowsAuthHandshakeCache();
        private static readonly Task _cachedTask = Task.FromResult(0);

        public WindowsAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<WindowsAuthenticationMiddleware>();
        }

        public Task Invoke(HttpContext httpContext)
        {
            var t = httpContext.Features.Get<IHttpConnectionFeature>();
            var authFeature = httpContext.Features.Get<WindowsAuthFeature>();

            if (authFeature == null || authFeature.Identity == null)
            {
                var sessionId = t.ConnectionId;

                var authorizationHeader = httpContext.Request.Headers["Authorization"];
                var tokenHeader = authorizationHeader.FirstOrDefault(h => h.StartsWith("NTLM ") || h.StartsWith("Negotiate "));
                if (string.IsNullOrEmpty(tokenHeader))
                {
                    httpContext.Response.Headers.Add("WWW-Authenticate", new[] { "Negotiate", "NTLM" });
                    httpContext.Response.StatusCode = 401;
                    return _cachedTask;
                }
                tokenHeader = tokenHeader.Substring(tokenHeader.IndexOf(' ') + 1);
                var token = Convert.FromBase64String(tokenHeader);

                var result = _cache.ProcessHandshake(token, sessionId);
                if (result != null)
                {
                    httpContext.Response.Headers.Add("WWW-Authenticate", new[] { result });
                }
                var user = _cache.GetUser(sessionId);
                if (user == null)
                {
                    httpContext.Response.StatusCode = 401;
                    httpContext.Response.ContentLength = 0;
                    return _cachedTask;
                }
                authFeature.Identity = user;
            }
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(authFeature.Identity);
            return _next(httpContext);
        }
    }
}
