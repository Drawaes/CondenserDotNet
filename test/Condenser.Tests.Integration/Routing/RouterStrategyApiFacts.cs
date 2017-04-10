using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{
    [Collection("RoutingTests")]
    public class RouterStrategyApiFacts
    {

        [Fact]
        public async Task CanCallRouterWithDifferentRouterStrategies()
        {
            using (var fixture = new RoutingFixture())
            {
                var serviceName1 = fixture.GetNewServiceName();
                var route1 = "/data";

                var strategyOne = new StrategyOne();
                var strategyTwo = new StrategyTwo();

                fixture.AddService(serviceName1, route1);

                fixture.AddRouter(strategyOne, strategyTwo);

                fixture.StartAll();

                await fixture.WaitForRegistrationAsync();
                
                var routerResponse = await fixture.CallRouterAsync("/admin/condenser/router/replace?strategy=One");
                Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);

                var responseService = await fixture.CallRouterAsync(route1);
                Assert.Equal(HttpStatusCode.OK, responseService.StatusCode);
                Assert.True(strategyOne.Called, "strategy one was not called");

                routerResponse = await fixture.CallRouterAsync("/admin/condenser/router/replace?strategy=Two");
                Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);

                responseService = await fixture.CallRouterAsync(route1);
                Assert.Equal(HttpStatusCode.OK, responseService.StatusCode);
                Assert.True(strategyTwo.Called, "strategy two was not called");
            }
        }

        public class StrategyOne : IRoutingStrategy<IService>
        {
            public string Name => "One";

            public bool Called { get; set; }

            public IService RouteTo(List<IService> services)
            {
                Called = true;
                return services[0];
            }
        }

        public class StrategyTwo : IRoutingStrategy<IService>
        {
            public string Name => "Two";

            public bool Called { get; set; }

            public IService RouteTo(List<IService> services)
            {
                Called = true;
                return services[0];
            }
        }
    }
}
