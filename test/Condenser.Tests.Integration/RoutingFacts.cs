using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Routes;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class RoutingFacts
    {
        private const string UrlPrefix = "urlprefix-";

        [Fact]
        public async Task CanWeFindARouteAndGetAPage()
        {
            var informationService = new InformationService
            {
                Address = "www.google.com",
                Port = 80
            };
            
            var router = BuildRouter();
            var service = new Service(null, null);
            await service.Initialise("service1", "node1", new[] { UrlPrefix + "/search" }, "www.google.com", 80);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";
                        
            var routedService = router.GetServiceFromRoute(context.Request.Path, out string matchedPath);
            await routedService.CallService(context);
                        
            Assert.Equal(200, context.Response.StatusCode);
        }

        
        [Fact(Skip = "Unsure if this could work without a health check?")]
        public async Task CanRegisterRoutes()
        {
            var wait = new AsyncManualResetEvent<bool>();

            var agentAddress = "localhost";
            var config = new CondenserConfiguration
            {
                AgentAddress = agentAddress,
                AgentPort = 8500
            };
            var router = BuildRouter();
           
            var host = new RoutingHost(router, config, null, RoutingData.BuildDefault(), 
                new IService[0], () => new Service(new CurrentState(), null))
            {
                OnRouteBuilt = servers =>
                {
                    {
                        if (servers.ContainsKey("Google"))
                        {
                            wait.Set(true);
                        }
                    }
                }
            };

            var google = new ServiceManager("Google",
                agentAddress, 8500)
            {
                ServiceAddress = "www.google.com",
                ServicePort = 80
            };

            await google.AddApiUrl("/search")
                .RegisterServiceAsync();
            
            await wait.WaitAsync();

            var routeContext = await RouteRequest(router, "/search");

            Assert.Equal(200, routeContext.HttpContext.Response.StatusCode);

            google = new ServiceManager("Google","Google2", agentAddress, 8500)
            {
                ServiceAddress = "www.google.com",
                ServicePort = 80
            };

            wait.Reset();

            await google.AddApiUrl("/gmail")
                .AddHttpHealthCheck("/gmail",10)
                .RegisterServiceAsync();

            await wait.WaitAsync();

            routeContext = await RouteRequest(router, "/gmail");

            Assert.Equal(200, routeContext.HttpContext.Response.StatusCode);

            routeContext = await RouteRequest(router, "/search");

            Assert.Null(routeContext);
        }

        private CustomRouter BuildRouter()
        {
            Func<ChildContainer<IService>> createNode = () =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy },
                    null));
            };
            var data = new RoutingData(new RadixTree<IService>(createNode));
            return new CustomRouter(null,
                data);
        }

        private class FakeHealthConfig : IHealthConfig
        {
            public FakeHealthConfig(List<Func<Task<HealthCheck>>> checks, string route)
            {
                Checks = checks;
                Route = route;
            }

            public List<Func<Task<HealthCheck>>> Checks { get; }
            public string Route { get; }
        }

        private static async Task<RouteContext> RouteRequest(CustomRouter router, 
            string requestPath)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = requestPath;

            var routeContext = new RouteContext(context);

            //await router.RouteAsync(routeContext);

            if (routeContext.Handler == null)
            {
                return null;
            }

            await routeContext.Handler.Invoke(routeContext.HttpContext);
            return routeContext;
        }
    }
}