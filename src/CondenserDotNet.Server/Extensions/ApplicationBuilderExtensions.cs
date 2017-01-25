using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseCondenserRouter(this IApplicationBuilder self)
        {
            var host = self.ApplicationServices.GetService<RoutingHostLite>();
            self.UseRouter(host.Router);
        }
    }
}