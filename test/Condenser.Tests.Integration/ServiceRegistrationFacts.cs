using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceRegistrationFacts
    {
        [Fact]
        public async Task TestRegister()
        {
            var serviceName = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager(serviceName))
            {
                manager.AddApiUrl("api/testurl");
                var registrationResult = await manager.RegisterServiceAsync();

                Assert.Equal(true, registrationResult);
            }
        }
        [Fact]
        public async Task TestRegisterAndSetPassTtl()
        {
            var serviceName = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager(serviceName))
            {
                manager.AddTtlHealthCheck(10);
                var registerResult = await manager.RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();

                Assert.Equal(true, ttlResult);
            }
        }
        [Fact]
        public async Task TestRegisterAndCheckRegistered()
        {
            var serviceName = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager(serviceName))
            {
                var registrationResult = await manager.RegisterServiceAsync();
                Assert.Equal(true, registrationResult);

                var services = await manager.Services.GetAvailableServicesAsync();

                Assert.Contains(serviceName, services);
            }
        }
    }
}
