using System;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CondenserDotNet.Server
{
    public interface IConfigurationBuilder
    {
        IConfigurationBuilder WithAgentAddress(string agentAdress);
        IConfigurationBuilder WithAgentPort(int agentPort);
        IConfigurationBuilder WithHealthRoute(string route);
        IConfigurationBuilder WithHealthCheck(Func<Task<HealthCheck>> check);
        IConfigurationBuilder WithHealthCheck(Func<HealthCheck> check);
        IConfigurationBuilder WithRoutingStrategy(RouteStrategy name);
        IConfigurationBuilder WithHttpClient(Func<string, HttpClient> clientFactory);

        IConfigurationBuilder WithRoutesBuiltCallback(Action<string[]> onRoutesBuilt);

        IConfigurationBuilder WithRoutingStrategies(IEnumerable<IRoutingStrategy<IService>> strategies);

        IServiceCollection Build();
    }
}
