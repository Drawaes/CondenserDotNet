using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace CondenserDotNet.Server
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCondenser(this IApplicationBuilder self)
        {
            self.UseMiddleware<RoutingMiddleware>();
            //self.UseMiddleware<WebsocketMiddleware>();
            self.UseMiddleware<ServiceCallMiddleware>();

            //Resolve to start building routes
            self.ApplicationServices.GetServices<RoutingHost>();

            return self;
        }
    }
}
