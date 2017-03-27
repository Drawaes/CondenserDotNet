using CondenserDotNet.Client;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ListCallbackFacts
    {
        [Fact]
        public async Task TestCallbackIsCalled()
        {
            var serviceName = Guid.NewGuid().ToString();
            var serviceCount = 100;
            var opts = Options.Create(new ServiceManagerConfig() { ServiceName = serviceName, ServicePort = 2222 });
            using (var manager = new ServiceManager(opts))
            using (var register = new ServiceRegistry())
            {
                register.SetServiceListCallback(serviceName, list =>
                {
                    Volatile.Write(ref serviceCount, list?.Count ?? 0);
                });
                await Task.Delay(500);
                Assert.Equal(0, serviceCount);

                manager.AddTtlHealthCheck(10);
                var registerResult = await manager.RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();

                await Task.Delay(500);
                Assert.Equal(1, serviceCount);

                ttlResult = await manager.TtlCheck.ReportFailAsync();

                await Task.Delay(500);
                Assert.Equal(0, serviceCount);
            }
        }


    }
}
