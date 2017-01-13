using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseCondenserRouter(this IApplicationBuilder self)
        {
            var host = self.ApplicationServices.GetService<RoutingHost>();
            //TODO:  Need to move this, get initial services
            host.Initialise().Wait();

            self.UseRouter(host.Router);
        }
    }
}