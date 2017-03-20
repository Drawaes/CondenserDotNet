using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class ServiceCallMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RoutingHost _routeData;

        public ServiceCallMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, RoutingHost routeData)
        {
            _next = next;
            _logger = loggerFactory?.CreateLogger<RoutingMiddleware>();
            _routeData = routeData;
        }

        public Task Invoke(HttpContext context)
        {
            var service = context.Features.Get<IService>();
            return service.CallService(context);
        }
    }
}
