using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Newtonsoft.Json;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceLookupFacts
    {
        [Fact]
        public async Task TestRegisterAndCheckRegistered()
        {
            var key = Guid.NewGuid().ToString();
            Console.WriteLine(nameof(TestRegisterAndCheckRegistered));
            using (var manager = new ServiceManager(key, key + "Id1"))
            {
                using (var manager2 = new ServiceManager(key, key + "Id2"))
                {
                    var registrationResult = await manager.RegisterServiceAsync();
                    Assert.Equal(true, registrationResult);

                    var registrationResult2 = await manager2.RegisterServiceAsync();
                    Assert.Equal(true, registrationResult2);

                    var service = await manager.Services.GetServiceInstanceAsync(key);
                    Assert.Equal(key, service.Service);
                }
            }
        }

        [Fact]
        public async Task TestRegisterAndCheckUpdates()
        {
            Console.WriteLine(nameof(TestRegisterAndCheckUpdates));
            var serviceName = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager(serviceName))
            {
                Assert.Null(await manager.Services.GetServiceInstanceAsync(serviceName));
                await manager.RegisterServiceAsync();
                //Give it 500ms to update with the new service
                await Task.Delay(500);
                Assert.NotNull(await manager.Services.GetServiceInstanceAsync(serviceName));
            }
        }
    }
}