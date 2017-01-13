using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CondenserDotNet.Server.Extensions
{
    public static class ServiceCollectionExtentions
    {
        public static void AddCondenserRouter(this IServiceCollection self,
            string agentAddress, int agentPort)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };

            self.AddRouting();
            self.AddSingleton(config);
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
        }
    }
}