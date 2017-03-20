using Microsoft.AspNetCore.Builder;

namespace CondenserDotNet.Server
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCondenser(this IApplicationBuilder self)
        {
            self.UseMiddleware<RoutingMiddleware>();
            //self.UseMiddleware<WebsocketMiddleware>();
            self.UseMiddleware<ServiceCallMiddleware>();
            return self;
        }
    }
}
