using System;
using CondenserDotNet.Server;
using CondenserTests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CondenserTests
{
    public class CustomRouterFacts
    {
        [Fact]
        public async void SetupCustomRouterAndRouteToService()
        {
            var router = BuildRouter();
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
            var router = BuildRouter();

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

        private CustomRouter BuildRouter()
        {
            return new CustomRouter(new FakeHealthRouter(), new FakeLogger<CustomRouter>());
        }

        public class FakeLogger<T> : ILogger<T>
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter)
            {

            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }
        }
    }
}
