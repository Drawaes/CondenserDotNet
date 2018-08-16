using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Configuration.Consul
{
    /// <summary>
    /// This manages the configuration keys you load as well as watching live keys and allowing you to add or update keys.
    /// </summary>
    public class ConsulRegistry : IConfigurationRegistry
    {
        private readonly List<Dictionary<string, string>> _configKeys = new List<Dictionary<string, string>>();
        private readonly List<ConfigurationWatcher> _configWatchers = new List<ConfigurationWatcher>();
        private readonly IConfigSource _source;
        private readonly ILogger _logger;
        private IConfigurationRoot _root;
        private readonly ConfigurationBuilder _builder = new ConfigurationBuilder();
        private readonly Core.Consul.IConsulAclProvider _aclProvider;

        public ConsulRegistry(IOptions<ConsulRegistryConfig> agentConfig, ILoggerFactory loggerFactory = null, Core.Consul.IConsulAclProvider aclProvider = null)
        {
            _aclProvider = aclProvider;
            _logger = loggerFactory?.CreateLogger<ConsulRegistry>();
            _source = new ConsulConfigSource(agentConfig, _logger, _aclProvider);
            _builder.AddConfigurationRegistry(this);
        }

        public ConsulRegistry(ILoggerFactory loggerFactory = null, Core.Consul.IConsulAclProvider aclProvider = null)
            :this(null, loggerFactory, aclProvider)
        {
            
        }

        /// <summary>
        /// This returns a flattened list of all the loaded keys
        /// </summary>
        public IEnumerable<string> AllKeys => _configKeys.SelectMany(x => x.Keys);
        public IConfigSource ConfigSource => _source;
        public IConfigurationRoot Root => _root ?? (_root = _builder.Build());
        public ConfigurationBuilder Builder => _builder;

        /// <summary>
        /// This loads the keys from a path. They are not updated.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public async Task<bool> AddStaticKeyPathAsync(string keyPath)
        {
            keyPath = _source.FormValidKey(keyPath);
            return (await AddInitialKeyPathAsync(keyPath)) > -1;
        }

        private async Task<int> AddInitialKeyPathAsync(string keyPath)
        {
            var response = await _source.GetKeysAsync(keyPath);

            if (!response.Success)
            {
                return -1;
            }

            return AddNewDictionaryToList(response.Dictionary);
        }

        /// <summary>
        /// This loads the keys from a path. The path is then watched and updates are added into the configuration set
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public async Task AddUpdatingPathAsync(string keyPath)
        {
            keyPath = _source.FormValidKey(keyPath);
            var initialDictionary = await AddInitialKeyPathAsync(keyPath);
            if (initialDictionary == -1)
            {
                var newDictionary = new Dictionary<string, string>();
                initialDictionary = AddNewDictionaryToList(newDictionary);
            }

            var ignore = WatchingLoop(initialDictionary, keyPath);
        }

        private async Task WatchingLoop(int indexOfDictionary, string keyPath)
        {
            var state = _source.CreateWatchState();

            try
            {
                while (true)
                {
                    var response = await _source.TryWatchKeysAsync(keyPath, state);

                    if (!response.Success)
                    {
                        continue;
                    }

                    UpdateDictionaryInList(indexOfDictionary, response.Dictionary);
                    FireWatchers();
                }
            }
            catch (TaskCanceledException) { /* nom nom */}
            catch (ObjectDisposedException) { /* nom nom */ }
        }

        private void FireWatchers()
        {
            lock (_configWatchers)
            {
                foreach (var watch in _configWatchers)
                {
                    if (watch.KeyToWatch == null)
                    {
                        Task.Run(watch.CallbackAllKeys);
                    }
                    else
                    {
                        TryGetValue(watch.KeyToWatch, out var newValue);
                        if (StringComparer.OrdinalIgnoreCase.Compare(watch.CurrentValue, newValue) != 0)
                        {
                            watch.CurrentValue = newValue;
                            Task.Run(() => watch.CallBack(newValue));
                        }
                    }
                }
            }
        }

        private int AddNewDictionaryToList(Dictionary<string, string> dictionaryToAdd)
        {
            lock (_configKeys)
            {
                _configKeys.Add(dictionaryToAdd);
                return _configKeys.Count - 1;
            }
        }

        private void UpdateDictionaryInList(int index, Dictionary<string, string> dictionaryToAdd)
        {
            lock (_configKeys)
            {
                _configKeys[index] = dictionaryToAdd;
            }
        }

        public string this[string key]
        {
            get
            {
                if (TryGetValue(key, out var returnValue))
                {
                    return returnValue;
                }
                throw new ArgumentOutOfRangeException($"Unable to find the key {key}");
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            lock (_configKeys)
            {
                for (var i = _configKeys.Count - 1; i >= 0; i--)
                {
                    if (_configKeys[i].TryGetValue(key, out value))
                    {
                        return true;
                    }
                }
                value = null;
                return false;
            }
        }

        public void AddWatchOnEntireConfig(Action callback)
        {
            lock (_configWatchers)
            {
                _configWatchers.Add(new ConfigurationWatcher() { CallbackAllKeys = callback });
            }
        }

        public void AddWatchOnSingleKey(string keyToWatch, Action<string> callback)
        {
            lock (_configWatchers)
            {
                lock (_configKeys)
                {
                    TryGetValue(keyToWatch, out var currentValue);
                    _configWatchers.Add(new ConfigurationWatcher() { CallBack = callback, KeyToWatch = keyToWatch, CurrentValue = currentValue });
                }
            }
        }

        /// <summary>
        /// This allows you to set a configuration key
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> SetKeyAsync(string keyPath, string value)
        {
            keyPath = StripFrontAndBackSlashes(keyPath);
            var response = await _source.TrySetKeyAsync(keyPath, value);

            return response;
        }

        public static string StripFrontAndBackSlashes(string inputString)
        {
            var startIndex = inputString.StartsWith("/") ? 1 : 0;
            return inputString.Substring(startIndex, (inputString.Length - startIndex) - (inputString.EndsWith("/") ? 1 : 0));
        }

        public void Dispose()
        {
            //Currently there is nothing to shutdown, but we should cleanup by writing some info
        }
    }
}
