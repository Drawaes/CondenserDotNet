using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Routes;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Server
{
    public class ConfigurationBuilder : IHealthConfig, IConfigurationBuilder, IRoutingConfig, IHttpClientConfig
    {
        private readonly IServiceCollection _collection;
        private string _agentAddress = "localhost";
        private int _agentPort = 8500;
        private Func<string, HttpClient> _clientFactory;

        public ConfigurationBuilder(IServiceCollection collection) => _collection = collection;

        public List<Func<Task<HealthCheck>>> Checks { get; } = new List<Func<Task<HealthCheck>>>();
        public string Route { get; private set; } = CondenserRoutes.HealthStats;
        public string DefaultRouteStrategy { get; private set; } = RouteStrategy.Random.ToString();

        private readonly List<IRoutingStrategy<IService>> _strategies = new List<IRoutingStrategy<IService>>();

        public Action<string[]> OnRoutesBuilt { get; private set; }

        public IConfigurationBuilder WithAgentAddress(string agentAdress)
        {
            _agentAddress = agentAdress;
            return this;
        }

        public IConfigurationBuilder WithRoutesBuiltCallback(Action<string[]> onRoutesBuilt)
        {
            OnRoutesBuilt = onRoutesBuilt;
            return this;
        }

        public IConfigurationBuilder WithAgentPort(int agentPort)
        {
            _agentPort = agentPort;
            return this;
        }

        public IConfigurationBuilder WithHealthRoute(string route)
        {
            Route = route;
            return this;
        }

        public IConfigurationBuilder WithHealthCheck(Func<Task<HealthCheck>> check)
        {
            Checks.Add(check);
            return this;
        }

        public IConfigurationBuilder WithHealthCheck(Func<HealthCheck> check)
        {
            Checks.Add(() => Task.FromResult(check()));
            return this;
        }

        public IServiceCollection Build() => _collection.AddCondenser(_agentAddress, _agentPort, this, this, this, _strategies);

        public IConfigurationBuilder WithRoutingStrategy(RouteStrategy name)
        {
            DefaultRouteStrategy = name.ToString();
            return this;
        }

        public IConfigurationBuilder WithHttpClient(Func<string, HttpClient> clientFactory)
        {
            _clientFactory = clientFactory;
            return this;
        }

        public IConfigurationBuilder WithRoutingStrategies(IEnumerable<IRoutingStrategy<IService>> strategies)
        {
            _strategies.AddRange(strategies);
            return this;
        }

        public HttpClient Create(string serviceId) => _clientFactory?.Invoke(serviceId);
    }
}