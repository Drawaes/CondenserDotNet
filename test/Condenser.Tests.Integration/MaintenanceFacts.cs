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
    public class MaintenanceFacts
    {
        [Fact]
        public async Task TestIntoAndOutOfMaintenance()
        {
            var config = Options.Create(new ServiceManagerConfig());
            config.Value.ServiceAddress = "127.0.0.1";
            config.Value.ServicePort = 777;
            config.Value.ServiceName = Guid.NewGuid().ToString();
            var manager = new ServiceManager(config);
            await manager.RegisterServiceAsync();
            var registry = new ServiceRegistry();
            var instance = await registry.GetServiceInstanceAsync(config.Value.ServiceName);
            Assert.NotNull(instance);

            await manager.EnableMaintenanceModeAsync("down");
            await Task.Delay(200);

            instance = await registry.GetServiceInstanceAsync(config.Value.ServiceName);
            Assert.Null(instance);

            await manager.DisableMaintenanceModeAsync();
            await Task.Delay(200);
            instance = await registry.GetServiceInstanceAsync(config.Value.ServiceName);
            Assert.NotNull(instance);
        }
    }
}
