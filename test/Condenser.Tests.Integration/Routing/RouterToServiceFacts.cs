using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{
    [Collection("RoutingTests")]
    public class RouterToServiceFacts
    {
        [Fact]
        public async Task CanWeRunRegisteredServicesThroughRouter()
        {
            using (var fixture = new RoutingFixture())
            {

                var serviceName1 = fixture.GetNewServiceName();
                var route1 = "/test1";
                var serviceName2 = fixture.GetNewServiceName();
                var route2 = "/test2";

                fixture.AddService(serviceName1, route1)
                    .AddService(serviceName2, route2);

                fixture.AddRouter();

                fixture.StartAll();

                if (!fixture.AreAllRegistered())
                {
                    await fixture.WaitForRegistrationAsync();
                }

                var response = await fixture.CallRouterAsync(route1);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.Equal("Called me " + serviceName1, content);

                response = await fixture.CallRouterAsync(route2);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                content = await response.Content.ReadAsStringAsync();
                Assert.Equal("Called me " + serviceName2, content);
            }
        }
    }
}
