using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceDeregistrationFacts
    {
        [Fact]
        public async Task CheckDeregistrationWorks()
        {
            var serviceName = Guid.NewGuid().ToString();
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222, ServiceName = serviceName });
            using (var manager = new ServiceManager(opts))
            using (var registry = new ServiceRegistry())
            {
                var registrationResult = await manager.RegisterServiceAsync();
                Assert.True(registrationResult);

                var services = await registry.GetAvailableServicesAsync();
                Assert.Contains(serviceName, services);

                registrationResult = await manager.DeregisterServiceAsync();
                Assert.True(registrationResult);

                services = await registry.GetAvailableServicesAsync();
                Assert.DoesNotContain(serviceName, services);
            }
        }
    }
}
