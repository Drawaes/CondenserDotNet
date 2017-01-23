using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using Microsoft.AspNetCore.Hosting;

namespace CondenserDotNet.Server.Builder
{
    public class HealthCheckBuilder : IHealthConfig
    {
        private readonly IWebHostBuilder _builder;

        public static IHealthConfig NoHealth()
        {
            return new HealthCheckBuilder(null);
        }


        public HealthCheckBuilder(IWebHostBuilder builder)
        {
            _builder = builder;
        }

        public List<Func<Task<HealthCheck>>> Checks { get; } = new List<Func<Task<HealthCheck>>>();

        public string Route { get; private set; }

        public HealthCheckBuilder WithHealthRoute(string route)
        {
            Route = route;
            return this;
        }

        public HealthCheckBuilder WithHealthCheck(Func<Task<HealthCheck>> check)
        {
            Checks.Add(check);
            return this;
        }

        public HealthCheckBuilder WithHealthCheck(Func<HealthCheck> check)
        {
            Checks.Add(() => Task.FromResult(check()));
            return this;
        }

        public IWebHost Build()
        {
            return _builder.Build();
        }
    }
}