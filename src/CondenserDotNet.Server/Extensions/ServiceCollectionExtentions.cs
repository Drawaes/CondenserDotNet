using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.Routes;
using Microsoft.Extensions.DependencyInjection;

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
            self.AddSingleton<RoutingData>();
            self.AddSingleton(config);
            self.AddSingleton<IService, HealthRouter>();
            self.AddSingleton<IService, RouteSummary>();
            self.AddSingleton<IService, TreeRouter>();
            self.AddSingleton<CurrentState>();
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
        }
    }
}