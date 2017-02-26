using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.Options;
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
            var opts = Options.Create(new ServiceManagerConfig() { ServiceName = key, ServiceId = key + "Id1"});
            var opts2 = Options.Create(new ServiceManagerConfig() { ServiceName = key, ServiceId = key + "Id2"});
            using (var manager = new ServiceManager(opts, new ServiceRegistry(() => new HttpClient())))
            {
                using (var manager2 = new ServiceManager(opts2, new ServiceRegistry(() => new HttpClient())))
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
            var opts = Options.Create(new ServiceManagerConfig() { ServiceName = serviceName, ServiceId = serviceName });
            using (var manager = new ServiceManager(opts, new ServiceRegistry(() => new HttpClient())))
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