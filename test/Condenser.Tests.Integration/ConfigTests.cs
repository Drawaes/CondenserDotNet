using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ConfigTests
    {
        [Fact]
        public async Task TestRegister()
        {
            var manager = new ServiceManager("TestService");
            await manager.Config.SetKeyAsync("org/test/test1", "testValue1");
            await manager.Config.SetKeyAsync("org/test/test2", "testValue2");

            var result = await manager.Config.AddStaticKeyPathAsync("org/test/");

            var firstValue = manager.Config["test1"];
            var secondValue = manager.Config["test2"];
            
            Assert.Equal("testValue1", firstValue);
            Assert.Equal("testValue2", secondValue);
        }
    }
}
