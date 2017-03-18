using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{

    public class RouterToServiceFacts
    {
        [Fact]
        public async Task CanWeRunRegisteredServicesThroughRouter()
        {
            using (var fixture = new RoutingFixture())
            {

                var serviceName1 = fixture.GetNewServiceName();
                var route1 = "/myservice1";

                fixture.AddService(serviceName1, route1);
                fixture.AddRouter();

                fixture.StartAll();

                await fixture.WaitForRegistrationAsync();

                var response = await fixture.CallRouterAsync("/myservice1");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.Equal("Called me " + serviceName1, content);
            }
        }
    }
}
