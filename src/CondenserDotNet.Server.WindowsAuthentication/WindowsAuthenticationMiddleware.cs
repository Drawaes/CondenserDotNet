using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private const string AuthorizationHeader = "Authorization";
        private const string NtlmToken = "NTLM ";
        private const string NegotiateToken = "Negotiate ";
        private const string WWWAuthenticateHeader = "WWW-Authenticate";
        private static readonly string[] _supportedTokens = new[] { "NTLM", "Negotiate" };
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

            if (authFeature == null)
            {
                throw new InvalidOperationException("You need the connection filter installed to use windows authentication");
            }
            
            if (authFeature.Identity == null)
            {
                var sessionId = t.ConnectionId;

                var authorizationHeader = httpContext.Request.Headers[AuthorizationHeader];
                var tokenHeader = authorizationHeader.FirstOrDefault(h => h.StartsWith(NtlmToken) || h.StartsWith(NegotiateToken));
                if (string.IsNullOrEmpty(tokenHeader))
                {
                    httpContext.Response.Headers.Add(WWWAuthenticateHeader, _supportedTokens);
                    httpContext.Response.StatusCode = 401;
                    return _cachedTask;
                }
                tokenHeader = tokenHeader.Substring(tokenHeader.IndexOf(' ') + 1);
                var token = Convert.FromBase64String(tokenHeader);
                string result = null;
                try
                {
                    result = _cache.ProcessHandshake(token, sessionId);
                }
                catch
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                if (result != null)
                {
                    httpContext.Response.Headers.Add(WWWAuthenticateHeader, new[] { result });
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
