using System.Collections.Generic;
using CondenserDotNet.Configuration;
using CondenserTests.Fakes;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CondenserTests
{
    public class ConsulConfigurationProviderTests
    {
        [Fact]
        public void CanBindConfiguratonSection()
        {
            var registry = new FakeConfigurationRegistry();
            registry.SetKeyAsync("FakeConfig:Setting1", "abc");
            registry.SetKeyAsync("FakeConfig:Setting2", "def");

            var builder = new ConfigurationBuilder()
                .AddConfigurationRegistry(registry)
                .Build();

            var config = new FakeConfig();
            builder.GetSection("FakeConfig").Bind(config);

            Assert.Equal("abc", config.Setting1);
            Assert.Equal("def", config.Setting2);
        }

        [Fact]
        public void CanBindConfiguratonSectionAsList()
        {
            var registry = new FakeConfigurationRegistry();
            registry.SetKeyAsync("my/config/section/objectlist:0:Setting1", "1");
            registry.SetKeyAsync("my/config/section/objectlist:0:Setting2", "2");
            registry.SetKeyAsync("my/config/section/objectlist:1:Setting1", "3");
            registry.SetKeyAsync("my/config/section/objectlist:1:Setting2", "4");

            var builder = new ConfigurationBuilder()
                .AddConfigurationRegistry(registry)
                .Build();

            var config = new List<FakeConfig>();
            builder.GetSection("my/config/section/objectlist").Bind(config);

            Assert.Equal(2, config.Count);
            Assert.Equal("1", config[0].Setting1);
            Assert.Equal("2", config[0].Setting2);
            Assert.Equal("3", config[1].Setting1);
            Assert.Equal("4", config[1].Setting2);
        }

        [Fact]
        public void CanGetConfig()
        {
            const string key = "key1";
            const string keyValue = "value1";

            var registry = new FakeConfigurationRegistry();
            var sut = new ConfigurationRegistryProvider(registry);

            registry.SetKeyAsync(key, keyValue);

            sut.TryGet(key, out var value);
            Assert.Equal(keyValue, value);
        }


        [Fact]
        public void CanReloadConfig()
        {
            var reloaded = false;

            var registry = new FakeConfigurationRegistry();
            var sut = new ConfigurationRegistryProvider(registry);

            sut.Load();
            sut.GetReloadToken().RegisterChangeCallback(_ => reloaded = true, null);

            registry.FakeReload();

            Assert.True(reloaded);
        }

        [Fact]
        public void CanSetConfig()
        {
            const string key = "key1";
            const string keyValue = "value1";

            var registry = new FakeConfigurationRegistry();
            var sut = new ConfigurationRegistryProvider(registry);

            sut.Set(key, keyValue);

            sut.TryGet(key, out var value);
            Assert.Equal(keyValue, value);
        }
    }
}
