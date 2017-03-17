using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{

    public class RouterToServiceFacts : IClassFixture<RoutingFixture>
    {
        RoutingFixture _fixture;

        public RouterToServiceFacts(RoutingFixture fixture)
        {
            this._fixture = fixture;
        }

        [Fact]
        public async Task CanWeRunRegisteredServicesThroughRouter()
        {
            var serviceName1 = _fixture.GetNewServiceName();
            var route1 = "/myservice1";

            _fixture.AddService(serviceName1, route1);
            _fixture.AddRouter();

            _fixture.StartAll();

            await _fixture.WaitForRegistrationAsync();

            var response = await _fixture.CallRouterAsync("/myservice1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Called me " + serviceName1, content);
        }
    }
}
