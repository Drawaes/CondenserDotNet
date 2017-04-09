using System;
using System.Net.Http;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.Routes;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.Extensions.DependencyInjection;
using CondenserDotNet.Server;
using System.Collections.Generic;

namespace CondenserDotNet.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCondenser(this IServiceCollection self) => AddCondenser(self, "localhost", 8500);

        private static IServiceCollection AddCondenser(this IServiceCollection self, string agentAddress, int agentPort) =>
            self.AddCondenserWithBuilder()
                .WithAgentAddress(agentAddress)
                .WithAgentPort(agentPort)
                .WithHttpClient(s => new HttpClient())
                .Build();

        public static IServiceCollection AddCondenser(this IServiceCollection self, string agentAddress, int agentPort,
            IHealthConfig health, IRoutingConfig routingConfig, IHttpClientConfig httpClientConfig,
            IEnumerable<IRoutingStrategy<IService>> strategies)
        {
            var config = new CondenserConfiguration
            {
                AgentPort = agentPort,
                AgentAddress = agentAddress
            };

            self.AddSingleton(config);
            self.AddSingleton(health);
            self.AddSingleton(routingConfig);
            self.AddSingleton<Func<string, HttpClient>>(httpClientConfig.Create);

            self.AddSingleton<RoutingData>();
            self.AddSingleton<IService, HealthStatsRouter>();
            self.AddSingleton<IService, HealthRouter>();
            self.AddSingleton<IRouteStore, RouteStore>();
            self.AddSingleton<IRouteSource, ConsulRouteSource>();
            self.AddSingleton<IService, RouteSummary>();
            self.AddSingleton<IService, TreeRouter>();
            self.AddSingleton<IService, ChangeRoutingStrategy>();
            self.AddSingleton<IService, ServerStatsRoute>();
            self.AddTransient<IRoutingStrategy<IService>, RandomRoutingStrategy<IService>>();
            self.AddTransient<IRoutingStrategy<IService>, RoundRobinRoutingStrategy<IService>>();
            self.AddSingleton<IDefaultRouting<IService>, DefaultRouting<IService>>();

            foreach(var service in strategies)
            {
                self.AddSingleton(service);
            }

            self.AddTransient<ChildContainer<IService>>();
            self.AddTransient<CurrentState>();
            self.AddSingleton<CustomRouter>();
            self.AddSingleton<RoutingHost>();
            self.AddSingleton<RadixTree<IService>>();

            self.AddTransient<Service>();
            self.AddSingleton<Func<IConsulService>>(x => x.GetService<Service>);
            self.AddSingleton<Func<ICurrentState>>(x => x.GetService<CurrentState>);
            self.AddSingleton<Func<ChildContainer<IService>>>(x => x.GetService<ChildContainer<IService>>);

            return self;
        }

        public static IConfigurationBuilder AddCondenserWithBuilder(this IServiceCollection self) => new ConfigurationBuilder(self);
    }
}
