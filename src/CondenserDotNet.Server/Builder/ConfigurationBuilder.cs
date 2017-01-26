using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server.Builder
{
    public class ConfigurationBuilder : IHealthConfig, IConfigurationBuilder
    {
        private readonly IWebHostBuilder _builder;
        private readonly List<Type> _preRoute = new List<Type>();
        private string _agentAddress = "localhost";
        private int _agentPort = 8500;
        private ILoggerProvider _logger;

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
                _builder.ConfigureServices(x => { x.AddCondenserRouter(_agentAddress, _agentPort, this); })
                    .Configure(x =>
                    {
                        foreach (var middleware in _preRoute)
                            x.UseMiddleware(middleware);

                        x.UseCondenserRouter();
                    })
                    .Build();
        }

        public List<Func<Task<HealthCheck>>> Checks { get; } = new List<Func<Task<HealthCheck>>>();

        public string Route { get; private set; }

        public IConfigurationBuilder UsePreRouteMiddleware<T>()
        {
            _preRoute.Add(typeof(T));
            return this;
        }
    }
}