using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.Configuration;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Client.Internal;
using CondenserDotNet.Core;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class ConfigurationRegistry : IConfigurationRegistry
    {
        private const string ConsulPath = "/";
        private const char ConsulPathChar = '/';
        private const char CorePath = ':';

        private readonly IServiceManager _serviceManager;
        private readonly List<Dictionary<string, string>> _configKeys = new List<Dictionary<string, string>>();
        private readonly List<ConfigurationWatcher> _configWatchers = new List<ConfigurationWatcher>();
        private IKeyParser _parser = SimpleKeyValueParser.Instance;

        internal ConfigurationRegistry(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public IEnumerable<string> AllKeys => _configKeys.SelectMany(x => x.Keys);
        public void UpdateKeyParser(IKeyParser parser)
        {
            _parser = parser;
        }


        public Task<bool> AddStaticKeyPathAsync(string keyPath)
        {
            if (!keyPath.EndsWith(ConsulPath)) keyPath = keyPath + ConsulPath;
            return AddInitialKeyPathAsync(keyPath).ContinueWith(r => r.Result > -1);
        }

        private async Task<int> AddInitialKeyPathAsync(string keyPath)
        {
            var response = await _serviceManager.Client.GetAsync($"{HttpUtils.KeyUrl}{keyPath}?recurse");
            if (!response.IsSuccessStatusCode)
            {
                return -1;
            }
            
            var dictionary = await BuildDictionaryAsync(keyPath, response);

            return AddNewDictionaryToList(dictionary);
        }

        public async Task AddUpdatingPathAsync(string keyPath)
        {
            if(!keyPath.EndsWith(ConsulPath)) keyPath = keyPath + ConsulPath;
            var initialDictionary = await AddInitialKeyPathAsync(keyPath);
            if (initialDictionary == -1)
            {
                var newDictionary = new Dictionary<string, string>();
                initialDictionary = AddNewDictionaryToList(newDictionary);
            }
            //We got values so lets start watching but we aren't waiting for this we will let it run
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            WatchingLoop(initialDictionary, keyPath);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task WatchingLoop(int indexOfDictionary, string keyPath)
        {
            try
            {
                var consulIndex = "0";
                string url = $"{HttpUtils.KeyUrl}{keyPath}?recurse&wait=300s&index=";
                while (true)
                {
                    var response = await _serviceManager.Client.GetAsync(url + consulIndex, _serviceManager.Cancelled);
                    consulIndex = response.GetConsulIndex();
                    if (!response.IsSuccessStatusCode)
                    {
                        //There is some error we need to do something 
                        continue;
                    }
                    var dictionary = await BuildDictionaryAsync(keyPath, response);
                    UpdateDictionaryInList(indexOfDictionary, dictionary);
                    FireWatchers();
                }
            }
            catch (TaskCanceledException) { /* nom nom */}
            catch (ObjectDisposedException) { /* nom nom */ }
        }

        private async Task<Dictionary<string, string>> BuildDictionaryAsync(string keyPath, HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var keys = JsonConvert.DeserializeObject<KeyValue[]>(content);

            var parsedKeys = keys.SelectMany(k => _parser.Parse(k));

            var dictionary = parsedKeys.ToDictionary(
                kv => kv.Key.Substring(keyPath.Length).Replace(ConsulPathChar, CorePath),
                kv => kv.IsDerivedKey ? kv.Value : kv.Value == null ? null : kv.ValueFromBase64(),
                StringComparer.OrdinalIgnoreCase);
            return dictionary;
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
                        string newValue;
                        TryGetValue(watch.KeyToWatch, out newValue);
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
            lock(_configKeys)
            { 
                _configKeys[index] = dictionaryToAdd;
            }
        }

        public string this[string key]
        {
            get
            {
                string returnValue;
                if (TryGetValue(key, out returnValue))
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
                for (int i = _configKeys.Count - 1; i >= 0; i--)
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
                    string currentValue;
                    TryGetValue(keyToWatch, out currentValue);
                    _configWatchers.Add(new ConfigurationWatcher() { CallBack = callback, KeyToWatch = keyToWatch, CurrentValue = currentValue });
                }
            }
        }

        public async Task<bool> SetKeyAsync(string keyPath, string value)
        {
            keyPath = HttpUtils.StripFrontAndBackSlashes(keyPath);
            var response = await _serviceManager.Client.PutAsync($"{HttpUtils.KeyUrl}{keyPath}", HttpUtils.GetStringContent(value));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
    }
}
