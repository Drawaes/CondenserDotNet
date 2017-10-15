using System;
using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{
    [Collection("RoutingTests")]
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
            var routingData = new RoutingData(null);
            var service = new Service(null, null, routingData);
            await service.Initialise("service1", "node1", new[] { UrlPrefix + "/search" }, "www.google.com", 80);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";

            var routedService = router.GetServiceFromRoute(context.Request.Path, out var matchedPath);
            await routedService.CallService(context);

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public async Task CanWeFindARouteAndGetAPageHttps()
        {
            var informationService = new InformationService
            {
                Address = "www.google.com",
                Port = 80
            };

            var router = BuildRouter();
            var routerData = new RoutingData(null);
            var service = new Service(null, null, routerData);
            await service.Initialise("service1", "node1", new[] { UrlPrefix + "/search", "protocolScheme-https" }, "www.google.com", 443);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";

            var routedService = router.GetServiceFromRoute(context.Request.Path, out var matchedPath);
            await routedService.CallService(context);
            Assert.Equal(200, context.Response.StatusCode);
        }

        private CustomRouter BuildRouter()
        {
            ChildContainer<IService> createNode()
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy },
                    null));
            }
            var data = new RoutingData(new RadixTree<IService>(createNode));
            return new CustomRouter(null, data, new IService[0]);
        }

    }
}
