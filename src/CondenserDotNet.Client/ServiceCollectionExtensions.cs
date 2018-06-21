using System;
using System.Net.Http;
using CondenserDotNet.Client.Leadership;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulServices(this IServiceCollection self)
        {
            self.AddSingleton<ILeaderRegistry, LeaderRegistry>();
            self.AddSingleton<IServiceRegistry, ServiceRegistry>();
            self.AddSingleton<IServiceManager, ServiceManager>();
            self.AddTransient<ServiceRegistryDelegatingHandler>();
            self.AddTransient<ServiceRegistryNearestDelegatingHandler>();
            return self;
        }
    }
}
