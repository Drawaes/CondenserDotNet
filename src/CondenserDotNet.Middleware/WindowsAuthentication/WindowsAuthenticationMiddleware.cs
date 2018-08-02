using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Middleware.WindowsAuthentication
{
    public class WindowsAuthenticationMiddleware
    {
        private const string AuthorizationHeader = "Authorization";
        private const string NtlmToken = "NTLM ";
        private const string NegotiateToken = "Negotiate ";
        private const string WWWAuthenticateHeader = "WWW-Authenticate";
        private static readonly string[] _supportedTokens = new[] { "NTLM", "Negotiate" };
        private readonly RequestDelegate _next;
        private readonly ILogger<WindowsAuthenticationMiddleware> _logger;
        private static readonly Task _cachedTask = Task.FromResult(0);

        public WindowsAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<WindowsAuthenticationMiddleware>();
        }

        public Task Invoke(HttpContext httpContext)
        {
            var t = httpContext.Features.Get<IHttpConnectionFeature>();
            var authFeature = httpContext.Features.Get<IWindowsAuthFeature>();

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
                var tokenName = tokenHeader.Substring(0, tokenHeader.IndexOf(' ') + 1);
                tokenHeader = tokenHeader.Substring(tokenHeader.IndexOf(' ') + 1);
                var token = Convert.FromBase64String(tokenHeader);
                string result = null;
                try
                {
                    result = authFeature.ProcessHandshake(tokenName, token);
                }
                catch
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                if (result != null)
                {
                    httpContext.Response.Headers.Add(WWWAuthenticateHeader, new[] { result });
                }
                var user = authFeature.GetUser();
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
