using CondenserDotNet.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CondenserTests
{
    public class ServiceManagerConfigurationFacts
    {
        [Fact]
        public void DefaultsOverrideCorrectlyTest()
        {
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222, ServiceName = "Test1", ServiceId = "ServiceId" });
            using (var manager = new ServiceManager(opts))
            {
                Assert.Equal(2222, manager.ServicePort);
                Assert.Equal("Test1", manager.ServiceName);
                Assert.Equal("ServiceId", manager.ServiceId);
            }
        }

        [Fact]
        public void DefaultsCorrectly()
        {
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222 });
            using (var manager = new ServiceManager(opts))
            {
                Assert.NotNull(manager.ServiceName);
                Assert.NotNull(manager.ServiceId);
            }
        }
    }
}
