using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Server;
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
            var router = new CustomRouter();
            var service = new Service(new string[] { "/search" }, "Service1", "www.google.com", 80, "node1", new string[0]);
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
