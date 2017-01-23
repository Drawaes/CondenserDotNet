using CondenserDotNet.Server.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server.Extensions
{
    public static class WebHostExtensions
    {
        public static HealthCheckBuilder AsCondenserRouter(this IWebHostBuilder self)
        {
            return self.AsCondenserRouter("localhost", 8500);
        }

        public static HealthCheckBuilder AsCondenserRouter(this IWebHostBuilder self, 
            string agentAddress, int port)
        {
            var healthBuilder = new HealthCheckBuilder(self);
            self.ConfigureServices(x =>
                {
                    x.AddCondenserRouter(agentAddress, port);
                    x.AddSingleton((IHealthConfig)healthBuilder);
                })
                .Configure(x => { x.UseCondenserRouter(); });

            return healthBuilder;


        }
    }
}