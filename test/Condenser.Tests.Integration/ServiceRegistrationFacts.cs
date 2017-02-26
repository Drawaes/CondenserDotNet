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
            var opts = Options.Create(new ServiceManagerConfig());
            using (var manager = new ServiceManager(opts, null))
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
            var opts = Options.Create(new ServiceManagerConfig());
            using (var manager = new ServiceManager(opts, null))
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
            var opts = Options.Create(new ServiceManagerConfig());
            using (var manager = new ServiceManager(opts, new ServiceRegistry(() => new HttpClient())))
            {
                var registrationResult = await manager.RegisterServiceAsync();
                Assert.Equal(true, registrationResult);

                var services = await manager.Services.GetAvailableServicesAsync();

                Assert.Contains(serviceName, services);
            }
        }
    }
}
