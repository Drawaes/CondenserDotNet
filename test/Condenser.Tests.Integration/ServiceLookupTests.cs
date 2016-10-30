using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceLookupTests
    {
        [Fact]
        public async Task TestRegisterAndCheckRegistered()
        {
            var manager = new ServiceManager("TestService3", "Id1");
            var registrationResult = await manager.RegisterServiceAsync();
            Assert.Equal(true, registrationResult);

            var manager2 = new ServiceManager("TestService3", "Id2");
            var registrationResult2 = await manager.RegisterServiceAsync();
            Assert.Equal(true, registrationResult2);

            var service = await manager.Services.GetServiceInstanceAsync("TestService3");

            Assert.Equal("TestService3", service.Service);
        }
    }
}
