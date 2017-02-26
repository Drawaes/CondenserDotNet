using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceRegistrationFacts
    {
        [Fact]
        public async Task TestRegister()
        {
            var serviceName = Guid.NewGuid().ToString();
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222 });
            using (var manager = new ServiceManager(opts))
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
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222 });
            using (var manager = new ServiceManager(opts))
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
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222, ServiceName = serviceName } );
            using (var manager = new ServiceManager(opts))
            using (var registry = new ServiceRegistry())
            {
                var registrationResult = await manager.RegisterServiceAsync();
                Assert.Equal(true, registrationResult);

                var services = await registry.GetAvailableServicesAsync();
                Assert.Contains(serviceName, services);
            }
        }
    }
}
