using CondenserDotNet.Core;
using CondenserDotNet.Server.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Condenser.Tests.Integration.Routing.RouterSumaryApiFacts;

namespace Condenser.Tests.Integration.Routing
{
    public class RouterStatisticsApiFacts
    {
        [Fact(Skip ="Broken")]
        public async Task CanCallRouterStatisticsForRegisteredService()
        {
            using (var fixture = new RoutingFixture())
            {
                var serviceName1 = fixture.GetNewServiceName();
                var route1 = "/customer";

                fixture.AddService(serviceName1, route1);
                fixture.AddRouter();

                fixture.StartAll();

                await fixture.WaitForRegistrationAsync();
                await Task.Delay(1500);
                var responseService = await fixture.CallRouterAsync(route1);
                Assert.Equal(HttpStatusCode.OK, responseService.StatusCode);

                var routerResponse = await fixture.CallRouterAsync("/admin/condenser/server/"+serviceName1);

                Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);

                var content = await routerResponse.Content.GetObject<ServerStats[]>();

                var server = content[0];
                Assert.Equal(1, server.Calls);
                Assert.Equal(DateTime.Now.Date, server.LastRequest.Date);
                Assert.True(server.LastRequestTime > 0, "last request time not recorded");
                Assert.True(server.AverageRequestTime > 0, "average request time not recorded");
            }
        }
    }
}
