using CondenserDotNet.Server.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CondenserDotNet.Server.Extensions
{
    public static class ServiceCollectionExtentions
    {
        public static void AddCondenserRouter(this IServiceCollection self)
        {
            self.AddCondenserRouter("localhost", 8500);
        }

        public static void AddCondenserRouter(this IServiceCollection self,
            string agentAddress, int agentPort)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };

            self.AddRouting();
            self.AddSingleton(HealthCheckBuilder.NoHealth());
            self.AddSingleton(config);
            self.AddSingleton<IHealthRouter, HealthRouter>();
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
        }
    }
}