using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CondenserTests
{
    public class ConsulConfigurationProviderTests
    {
        private class FakeConfigurationRegistry : IConfigurationRegistry
        {
            private readonly Dictionary<string, string> _data = new Dictionary<string, string>();
            private readonly List<Action> _reloadActions = new List<Action>();


            public string this[string key] => _data[key];

            public Task<bool> AddStaticKeyPathAsync(string keyPath)
            {
                throw new NotImplementedException();
            }

            public Task AddUpdatingPathAsync(string keyPath)
            {
                throw new NotImplementedException();
            }

            public void AddWatchOnEntireConfig(Action callback)
            {
                _reloadActions.Add(callback);
            }

            public void AddWatchOnSingleKey(string keyToWatch, Action<string> callback)
            {
                throw new NotImplementedException();
            }

            public void AddWatchOnSingleKey(string keyToWatch, Action callback)
            {
                throw new NotImplementedException();
            }

            public Task<bool> SetKeyAsync(string keyPath, string value)
            {
                _data.Add(keyPath, value);
                return Task.FromResult(true);
            }

            public bool TryGetValue(string key, out string value) => _data.TryGetValue(key, out value);
            public IEnumerable<string> AllKeys => _data.Keys;

            public void UpdateKeyParser(IKeyParser parser)
            {
            }


            public void FakeReload()
            {
                foreach (var action in _reloadActions)
                    action();
            }
        }

        private class FakeConfig
        {
            public string Setting1 { get; set; }
            public string Setting2 { get; set; }
        }

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