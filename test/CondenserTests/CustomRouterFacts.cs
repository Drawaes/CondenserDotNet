using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Server;
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
            var service = new Service(new string[] {"/test1/test2/test3/test4/test5" }, "Service1Test", "Address1Test", 10000, new string[0]);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test1/test2/test3/test4/test5/test6";
            var routeContext = new RouteContext(context);

            await router.RouteAsync(routeContext);

            Assert.Equal(service, routeContext.Handler.Target);
        }
    }
}
