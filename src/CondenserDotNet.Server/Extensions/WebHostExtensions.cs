using CondenserDotNet.Server.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CondenserDotNet.Server.Extensions
{
    public static class WebHostExtensions
    {
        public static IConfigurationBuilder AsCondenserRouter(this IWebHostBuilder self)
        {
            var healthBuilder = new ConfigurationBuilder(self);


            return healthBuilder;
        }
    }
}