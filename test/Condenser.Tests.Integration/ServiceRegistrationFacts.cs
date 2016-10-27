using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceRegistrationFacts
    {
        [Fact]
        public async Task TestSingleRegisterAndRetrieve()
        {
            var regClient = new CondenserDotNet.Client.ServiceRegistrationClient();
            regClient.Config(serviceName : "TestService", serviceId : Guid.NewGuid().ToString(), address : "localhost", port : 7777);
            regClient.AddUrls("api/testurl");
            await regClient.RegisterServiceAsync();

            //Now try to get it back
            var endpointClient = new CondenserDotNet.Client.ServiceEndpointClient();
            var serverAddress = await endpointClient.GetServiceAddressAsync("TestService");

            Assert.Equal("localhost",serverAddress.Item1);
            Assert.Equal(7777, serverAddress.Item2);
        }
    }
}
