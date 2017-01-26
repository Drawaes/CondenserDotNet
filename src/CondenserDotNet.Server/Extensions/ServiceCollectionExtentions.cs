using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CondenserDotNet.Server.Extensions
{
    public static class ServiceCollectionExtentions
    {
        internal static void AddCondenserRouter(this IServiceCollection self,
            string agentAddress, int agentPort, 
            IHealthConfig health)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };

            self.AddRouting();
            self.AddSingleton(health);
            self.AddSingleton(config);
            self.AddSingleton<IHealthRouter, HealthRouter>();
            self.AddSingleton<CurrentState>();
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
        }
    }
}