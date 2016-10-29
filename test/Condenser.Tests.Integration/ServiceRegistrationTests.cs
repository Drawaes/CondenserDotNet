using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceRegistrationTests
    {
        [Fact]
        public async Task TestRegister()
        {
            var manager = new ServiceManager("TestService");
            manager.AddApiUrl("api/testurl");
            var registrationResult = await manager.RegisterServiceAsync();

            Assert.Equal(true, registrationResult);
        }
        [Fact]
        public async Task TestRegisterAndSetPassTtl()
        {
            var manager = new ServiceManager("TestService");
            manager.AddTtlHealthCheck(10);
            var registerResult = await manager.RegisterServiceAsync();
            var ttlResult = await manager.TtlCheck.ReportPassingAsync();

            Assert.Equal(true, ttlResult);
        }
    }
}
