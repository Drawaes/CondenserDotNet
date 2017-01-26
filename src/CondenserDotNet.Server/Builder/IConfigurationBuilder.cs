using System;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server.Builder
{
    public interface IConfigurationBuilder
    {
        IConfigurationBuilder WithAgentAddress(string agentAdress);
        IConfigurationBuilder WithLogger(ILoggerProvider logger);
        IConfigurationBuilder WithAgentPort(int agentPort);
        IConfigurationBuilder WithHealthRoute(string route);
        IConfigurationBuilder WithHealthCheck(Func<Task<HealthCheck>> check);
        IConfigurationBuilder WithHealthCheck(Func<HealthCheck> check);
        IConfigurationBuilder UsePreRouteMiddleware<T>();
        IWebHost Build();
    }
}