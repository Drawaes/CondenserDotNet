using System.Threading.Tasks;
using CondenserDotNet.Server;
using CondenserDotNet.Service.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class RoutingFacts
    {
        [Fact]
        public async Task CanWeFindARouteAndGetAPage()
        {
            var registry = new FakeServiceRegistry();
            var informationService = new InformationService
            {
                Address = "www.google.com",
                Port = 80
            };
            registry.SetServiceInstance(informationService);

            var router = new CustomRouter();
            var service = new Service(new[] {"/search"}, "Service1",
                "node1", new string[0], registry);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";

            var routeContext = new RouteContext(context);

            await router.RouteAsync(routeContext);

            await routeContext.Handler.Invoke(routeContext.HttpContext);

            Assert.Equal(200, routeContext.HttpContext.Response.StatusCode);
        }
    }
}