using CondenserDotNet.Server;
using CondenserDotNet.Server.RoutingTrie;
using CondenserTests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace CondenserTests
{
    public class CustomRouterFacts
    {
        private const string UrlPrefix = "urlprefix-";

        [Fact]
        public async void SetupCustomRouterAndRouteToService()
        {
            var router = BuildRouter();
            var service = new Service(null, null);
            await service.Initialise("Service1Test", "node1", new string[] { UrlPrefix + "/test1/test2/test3/test4/test5" }, "Address1Test", 10000);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test1/test2/test3/test4/test5/test6";
            
            var routedService = router.GetServiceFromRoute(context.Request.Path, out string matchedPath);
            
            Assert.Equal(service, routedService);
        }

        [Fact]
        public async void SetupCustomRouterAndLookForbadRoute()
        {
            var router = BuildRouter();

            var service = new Service(null, null);
            await service.Initialise("Service1Test", "node1", new string[] { UrlPrefix + "/test1/test2/test3/test4/test5" }, "Address1Test", 10000);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test2/test2/test3/test4/test5/test6";
            var routeContext = new RouteContext(context);

            //await router.RouteAsync(routeContext);

            Assert.Null(routeContext.Handler);
        }

        private CustomRouter BuildRouter()
        {
            return new CustomRouter(null, RoutingData.BuildDefault());
        }
    }
}
