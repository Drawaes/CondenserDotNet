using CondenserDotNet.Server.Routes;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{

    [Collection("RoutingTests")]
    public class RouterApiFacts
    {
        [Fact]
        public async Task CanCallRouterHealthCheck()
        {
            using (var fixture = new RoutingFixture())
            {
                var serviceName1 = fixture.GetNewServiceName();
                var route1 = "/myservice3";

                fixture.AddService(serviceName1, route1);
                fixture.AddRouter();

                fixture.StartAll();

                await fixture.WaitForRegistrationAsync();

                var responseService = await fixture.CallRouterAsync("/myservice3");
                Assert.Equal(HttpStatusCode.OK, responseService.StatusCode);

                var routerResponse = await fixture.CallRouterAsync("/admin/condenser/health");

                Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);

                routerResponse = await fixture.CallRouterAsync("/admin/condenser/healthstats");

                Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);
                var content = await routerResponse.Content.ReadAsStringAsync();

                var health = JsonConvert.DeserializeObject<HealthResponse>(content);
                Assert.Equal(1, health.Stats.Http200Responses);
            }
        }
    }
}
