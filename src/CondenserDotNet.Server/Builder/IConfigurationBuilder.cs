using System;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.DataContracts;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server.Builder
{
    public interface IConfigurationBuilder
    {
        IConfigurationBuilder WithAgentAddress(string agentAdress);
        IConfigurationBuilder WithAgentPort(int agentPort);
        IConfigurationBuilder WithHealthRoute(string route);
        IConfigurationBuilder WithHealthCheck(Func<Task<HealthCheck>> check);
        IConfigurationBuilder WithHealthCheck(Func<HealthCheck> check);
        IConfigurationBuilder UsePreRouteMiddleware<T>();

        IConfigurationBuilder WithRoutingStrategy(RouteStrategy name);
        IConfigurationBuilder WithHttpClient(Func<string, HttpClient> clientFactory);
        IWebHost Build();
    }
}
