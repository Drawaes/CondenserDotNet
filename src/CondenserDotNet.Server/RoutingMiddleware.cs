using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class RoutingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RoutingHost _routeData;
        private static readonly Task _completedTask = Task.FromResult(0);

        public RoutingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, RoutingHost routeData)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<RoutingMiddleware>();
            _routeData = routeData;
        }

        public Task Invoke(HttpContext httpContext)
        {
            using (var scope = _logger?.BeginScope("Started request scope {path}", httpContext.Request.Path))
            {
                var service = _routeData.Router.GetServiceFromRoute(httpContext.Request.Path, out var matchedPath);
                if (service == null)
                {
                    _logger?.LogInformation("No matching route for {path}", httpContext.Request.Path);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return _completedTask;
                }
                _logger?.LogInformation("Found matching path {path} to service {serviceName}", httpContext.Request.Path, service.ServiceId);
                httpContext.Features.Set(service);
                httpContext.Items.Add("matchedPath", matchedPath);
                return _next.Invoke(httpContext);
            }
        }
    }
}
