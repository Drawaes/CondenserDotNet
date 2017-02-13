using System;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.Routes;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        internal static void AddCondenserRouter(this IServiceCollection self,
            string agentAddress, int agentPort, 
            IHealthConfig health, IRoutingConfig routingConfig, 
            IHttpClientConfig httpClientConfig)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };

            self.AddRouting();
            self.AddSingleton(health);
            self.AddSingleton(routingConfig);
            self.AddSingleton(httpClientConfig);

            self.AddSingleton<RoutingData>();
            self.AddSingleton(config);
            self.AddSingleton<IService, HealthRouter>();
            self.AddSingleton<IService, RouteSummary>();
            self.AddSingleton<IService, TreeRouter>();
            self.AddSingleton<IService, ChangeRoutingStrategy>();
            self.AddSingleton<IService, ServerStatsRoute>();
            self.AddTransient<IRoutingStrategy<IService>, RandomRoutingStrategy<IService>>();
            self.AddTransient<IRoutingStrategy<IService>, RoundRobinRoutingStrategy<IService>>();
            self.AddSingleton<IDefaultRouting<IService>,
                DefaultRouting<IService>>();

            self.AddTransient<ChildContainer<IService>>();
            self.AddSingleton<CurrentState>();
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
            self.AddSingleton<RadixTree<IService>>();

            self.AddTransient<Service>();
            self.AddSingleton<Func<IConsulService>>(x => x.GetService<Service>);
            self.AddSingleton<Func<ChildContainer<IService>>>(x => x.GetService<ChildContainer<IService>>);
        }
    }
}