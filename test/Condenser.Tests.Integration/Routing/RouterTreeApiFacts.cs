using CondenserDotNet.Server.DataContracts;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{
    [Collection("RoutingTests")]
    public class RouterTreeApiFacts
    {
        [Fact]
        public async Task CanCallRouterTreeForRegisteredService()
        {
            using (var fixture = new RoutingFixture())
            {
                var serviceName1 = fixture.GetNewServiceName();
                var route1 = "/myservice2";

                fixture.AddService(serviceName1, route1);
                fixture.AddRouter();

                fixture.StartAll();

                await fixture.WaitForRegistrationAsync();

                var responseService = await fixture.CallRouterAsync("/myservice2");
                Assert.Equal(HttpStatusCode.OK, responseService.StatusCode);

                var routerResponse = await fixture.CallRouterAsync("/admin/condenser/tree");

                Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);
                var content = await routerResponse.Content.ReadAsStringAsync();

                var node = JsonConvert.DeserializeObject<Node>(content);

                Assert.NotNull(node);
            }
        }
    }
}
