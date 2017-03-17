using CondenserDotNet.Server.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration.Routing
{
    public class RouterTreeApiFacts : IClassFixture<RoutingFixture>
    {
        RoutingFixture _fixture;

        public RouterTreeApiFacts(RoutingFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CanCallRouterTreeForRegisteredService()
        {
            var serviceName1 = _fixture.GetNewServiceName();
            var route1 = "/myservice2";

            _fixture.AddService(serviceName1, route1);
            _fixture.AddRouter();

            _fixture.StartAll();

            await _fixture.WaitForRegistrationAsync();

            var responseService = await _fixture.CallRouterAsync("/myservice2");
            Assert.Equal(HttpStatusCode.OK, responseService.StatusCode);

            var routerResponse = await _fixture.CallRouterAsync("/admin/condenser/tree");

            Assert.Equal(HttpStatusCode.OK, routerResponse.StatusCode);
            var content = await routerResponse.Content.ReadAsStringAsync();
            
            //var node = JsonConvert.DeserializeObject<Node>(content);
            Assert.Contains(route1.ToUpper(), content);
        }
    }
}
