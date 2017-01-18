using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Server;
using CondenserTests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace CondenserTests
{
    public class CustomRouterFacts
    {
        [Fact]
        public async void SetupCustomRouterAndRouteToService()
        {
            var router = new CustomRouter();
            var registry = new FakeServiceRegistry();
            registry.AddServiceInstance("Address1Test", 10000);
            var service = new Service(new string[] {"/test1/test2/test3/test4/test5" }, 
                "Service1Test", "node1", new string[0], registry);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test1/test2/test3/test4/test5/test6";
            var routeContext = new RouteContext(context);

            await router.RouteAsync(routeContext);

            Assert.Equal(service, routeContext.Handler.Target);
        }

        [Fact]
        public async void SetupCustomRouterAndLookForbadRoute()
        {
            var router = new CustomRouter();

            var registry = new FakeServiceRegistry();
            registry.AddServiceInstance("Address1Test", 10000);
            var service = new Service(new string[] { "/test1/test2/test3/test4/test5" }, "Service1Test", "node1", new string[0],
                registry);

            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test2/test2/test3/test4/test5/test6";
            var routeContext = new RouteContext(context);

            await router.RouteAsync(routeContext);

            Assert.Null(routeContext.Handler);
        }
    }
}
