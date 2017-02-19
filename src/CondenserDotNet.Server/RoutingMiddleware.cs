using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class RoutingMiddleware
    {
        private RequestDelegate _next;
        private ILogger _logger;
        private RoutingHost _routeData;
        private static Task _completedTask = Task.FromResult(0);

        public RoutingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, RoutingHost routeData)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<RoutingMiddleware>();
            _routeData = routeData;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var service = _routeData.Router.GetServiceFromRoute(httpContext.Request.Path, out string matchedPath);
            if(service == null)
            {
                _logger?.LogTrace("No matching route for {path}", httpContext.Request.Path);
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return _completedTask;
            }
            httpContext.Features.Set(service);
            httpContext.Items.Add("matchedPath",matchedPath);
            return _next.Invoke(httpContext);
        }
    }
}
