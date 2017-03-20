using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CondenserDotNet.Middleware.CleanShutdown
{
    public class CleanShutdownMiddleware
    {
        private RequestDelegate _next;
        private CleanShutdownService _shutdownService;

        public CleanShutdownMiddleware(RequestDelegate next, CleanShutdownService shutdownService)
        {
            _next = next;
            _shutdownService = shutdownService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            _shutdownService.StartRequest();
            try
            {
                await _next.Invoke(httpContext);
            }
            finally
            {
                _shutdownService.FinishRequest();
            }
        }
    }
}
