using CondenserDotNet.Server.Routes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{
    public class RouterApiFacts : IClassFixture<RoutingFixture>
    {
        RoutingFixture _fixture;

        public RouterApiFacts(RoutingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CanCallRouterHealthCheck()
        {
            var serviceName1 = _fixture.GetNewServiceName();
            var route1 = "/myservice3";

            _fixture.AddService(serviceName1, route1);
            _fixture.AddRouter();

            _fixture.StartAll();

            await _fixture.WaitForRegistrationAsync();

            var responseService = await _fixture.CallRouterAsync("/myservice3");
            Assert.Equal(HttpStatusCode.OK, responseService.StatusCode);

            var routerResponse = await _fixture.CallRouterAsync("/admin/condenser/health");

            Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);
            var content = await routerResponse.Content.ReadAsStringAsync();

            var health =JsonConvert.DeserializeObject<HealthResponse>(content);
            Assert.Equal(1, health.Stats.Http200Responses);
        }
    }    
}
