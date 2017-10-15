using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Configuration;
using Microsoft.Extensions.Configuration;

namespace CondenserTests.Fakes
{
    internal class FakeConfigurationRegistry : IConfigurationRegistry
    {
        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();
        private readonly List<Action> _reloadActions = new List<Action>();


        public FakeConfigurationRegistry() => Root = new ConfigurationBuilder()
                .AddConfigurationRegistry(this)
                .Build();

        public string this[string key] => _data[key];

        public Task<bool> AddStaticKeyPathAsync(string keyPath) => throw new NotImplementedException();

        public Task AddUpdatingPathAsync(string keyPath) => throw new NotImplementedException();

        public void AddWatchOnEntireConfig(Action callback) => _reloadActions.Add(callback);

        public void AddWatchOnSingleKey(string keyToWatch, Action<string> callback) => throw new NotImplementedException();

        public Task<bool> SetKeyAsync(string keyPath, string value)
        {
            _data[keyPath] = value;
            return Task.FromResult(true);
        }

        public bool TryGetValue(string key, out string value) => _data.TryGetValue(key, out value);
        public IEnumerable<string> AllKeys => _data.Keys;

        public IConfigurationRoot Root { get; }

        public void AddWatchOnSingleKey(string keyToWatch, Action callback) => throw new NotImplementedException();


        public void FakeReload()
        {
            foreach (var action in _reloadActions)
                action();
        }

        public void Dispose()
        {

        }
    }
}
