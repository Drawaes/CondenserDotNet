using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CondenserDotNet.Client.Leadership;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulServices(this IServiceCollection self)
        {
            self.AddSingleton<Func<HttpClient>>(() => new HttpClient() { BaseAddress = new Uri($"http://localhost:8500"), Timeout = new TimeSpan(0, 6, 0) });
            self.AddSingleton<ILeaderRegistry, LeaderRegistry>();
            self.AddSingleton<IServiceRegistry, ServiceRegistry>();
            self.AddSingleton<IServiceManager, ServiceManager>();
            return self;
        }
    }
}
