using CondenserDotNet.Client;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ListCallbackFacts
    {
        [Fact]
        public async Task TestCallbackIsCalled()
        {
            var autoReset = new System.Threading.AutoResetEvent(false);
            var serviceName = Guid.NewGuid().ToString();
            var serviceCount = 100;
            var opts = Options.Create(new ServiceManagerConfig() { ServiceName = serviceName, ServicePort = 2222 });
            using (var manager = new ServiceManager(opts))
            using (var register = new ServiceRegistry())
            {
                register.SetServiceListCallback(serviceName, list =>
                {
                    serviceCount = list.Count;
                    autoReset.Set();
                });
                autoReset.WaitOne(5000);
                Assert.Equal(0, serviceCount);

                manager.AddTtlHealthCheck(10);
                var registerResult = await manager.RegisterServiceAsync();
                autoReset.Reset();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();

                autoReset.WaitOne(5000);
                Assert.Equal(1, serviceCount);

                ttlResult = await manager.TtlCheck.ReportFailAsync();
                autoReset.WaitOne(5000);
                Assert.Equal(0, serviceCount);
            }
        }


    }
}
