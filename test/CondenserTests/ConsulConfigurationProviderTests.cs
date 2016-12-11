using CondenserDotNet.Client.Configuration;
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
                .AddJsonConsul(registry)
                .Build();

            var config = new FakeConfig();
            builder.GetSection("FakeConfig").Bind(config);

            Assert.Equal("abc", config.Setting1);
            Assert.Equal("def", config.Setting2);
        }

        [Fact]
        public void CanGetConfig()
        {
            const string key = "key1";
            const string keyValue = "value1";

            var registry = new FakeConfigurationRegistry();
            var sut = new ConsulProvider(registry);

            registry.SetKeyAsync(key, keyValue);

            string value;
            sut.TryGet(key, out value);
            Assert.Equal(keyValue, value);
        }


        [Fact]
        public void CanReloadConfig()
        {
            var reloaded = false;

            var registry = new FakeConfigurationRegistry();
            var sut = new ConsulProvider(registry);

            sut.Load();

            sut.GetReloadToken()
                .RegisterChangeCallback(_ => reloaded = true, null);

            registry.FakeReload();

            Assert.True(reloaded);
        }

        [Fact]
        public void CanSetConfig()
        {
            const string key = "key1";
            const string keyValue = "value1";

            var registry = new FakeConfigurationRegistry();
            var sut = new ConsulProvider(registry);

            sut.Set(key, keyValue);

            string value;
            sut.TryGet(key, out value);
            Assert.Equal(keyValue, value);
        }
    }
}