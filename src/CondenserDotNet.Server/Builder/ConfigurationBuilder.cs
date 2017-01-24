using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server.Builder
{
    public class ConfigurationBuilder : IHealthConfig
    {
        private readonly IWebHostBuilder _builder;
        private ILoggerProvider _logger;

        private string _agentAddress = "localhost";
        private int _agentPort = 8500;

        public ConfigurationBuilder(IWebHostBuilder builder)
        {
            _builder = builder;
        }

        public List<Func<Task<HealthCheck>>> Checks { get; } = new List<Func<Task<HealthCheck>>>();

        public string Route { get; private set; }

        public ConfigurationBuilder WithAgentAddress(string agentAdress)
        {
            _agentAddress = agentAdress;
            return this;
        }

        public ConfigurationBuilder WithLogger(ILoggerProvider logger)
        {
            _logger = logger;
            return this;
        }

        public ConfigurationBuilder WithAgentPort(int agentPort)
        {
            _agentPort = agentPort;
            return this;
        }


        public ConfigurationBuilder WithHealthRoute(string route)
        {
            Route = route;
            return this;
        }

        public ConfigurationBuilder WithHealthCheck(Func<Task<HealthCheck>> check)
        {
            Checks.Add(check);
            return this;
        }

        public ConfigurationBuilder WithHealthCheck(Func<HealthCheck> check)
        {
            Checks.Add(() => Task.FromResult(check()));
            return this;
        }

        public IWebHost Build()
        {
            return
                _builder.ConfigureServices(x => { x.AddCondenserRouter(_agentAddress, _agentPort, this); })
                    .ConfigureLogging(x =>
                    {
                        if (_logger != null)
                        {
                            x.AddProvider(_logger);
                        }
                        else
                        {
                            x.AddConsole().AddDebug();
                        }
                    })
                    .Configure(x => { x.UseCondenserRouter(); })
                    .Build();
        }
    }
}