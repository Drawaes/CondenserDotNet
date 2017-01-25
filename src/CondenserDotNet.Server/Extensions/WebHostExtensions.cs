using CondenserDotNet.Server.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server.Extensions
{
    public static class WebHostExtensions
    {
        public static ConfigurationBuilder AsCondenserRouter(this IWebHostBuilder self)
        {
            return self.AsCondenserRouter("localhost", 8500);
        }

        public static ConfigurationBuilder AsCondenserRouter(this IWebHostBuilder self, 
            string agentAddress, int port)
        {
            var healthBuilder = new ConfigurationBuilder(self);
            

            return healthBuilder;


        }
    }
}