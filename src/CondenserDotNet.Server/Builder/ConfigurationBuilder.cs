using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using CondenserDotNet.Server.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace CondenserDotNet.Server.Builder
{
    public class ConfigurationBuilder : IHealthConfig, IConfigurationBuilder,
        IRoutingConfig, IHttpClientConfig
    {
        private readonly IWebHostBuilder _builder;
        private readonly List<Type> _preRoute = new List<Type>();
        private string _agentAddress = "localhost";
        private int _agentPort = 8500;
        private Func<string, HttpClient> _clientFactory;

        public ConfigurationBuilder(IWebHostBuilder builder)
        {
            _builder = builder;
        }

        public IConfigurationBuilder WithAgentAddress(string agentAdress)
        {
            _agentAddress = agentAdress;
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

        public IWebHost Build()
        {
            return
                _builder.ConfigureServices(x =>
                    {
                        x.AddCondenserRouter(_agentAddress, _agentPort, this, this, this);
                    })
                    .Configure(x =>
                    {
                        foreach (var middleware in _preRoute)
                        {
                            x.UseMiddleware(middleware);
                        }
                        x.UseCondenserRouter();
                    })
                    .Build();
        }

        public IConfigurationBuilder UsePreRouteMiddleware<T>()
        {
            _preRoute.Add(typeof(T));
            return this;
        }

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

        public List<Func<Task<HealthCheck>>> Checks { get; } = new List<Func<Task<HealthCheck>>>();

        public string Route { get; private set; } = CondenserRoutes.Health;

        public string DefaultRouteStrategy { get; private set; } = RouteStrategy.Random.ToString();
        public HttpClient Create(string serviceId)
        {
            return _clientFactory?.Invoke(serviceId);
        }
    }
}