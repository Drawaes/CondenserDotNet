using System;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.Routes;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCondenser(this IServiceCollection self)
        {
            return AddCondenser(self, "localhost", 8500);
        }

        private static IServiceCollection AddCondenser(this IServiceCollection self, string agentAddress, int agentPort)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };
            self.AddSingleton(config);
            self.AddTransient<Service>();
            self.AddSingleton<Func<IConsulService>>(x => x.GetService<Service>);
            self.AddTransient<IRoutingStrategy<IService>, RandomRoutingStrategy<IService>>();
            self.AddTransient<IRoutingStrategy<IService>, RoundRobinRoutingStrategy<IService>>();
            self.AddSingleton<IDefaultRouting<IService>, DefaultRouting<IService>>();
            Func<ChildContainer<IService>> factory = () =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy }, null));
            };
            self.AddSingleton(new RoutingData(new RadixTree<IService>(factory)));
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
            return self;
        }
    }
}