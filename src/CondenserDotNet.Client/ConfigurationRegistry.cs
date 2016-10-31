using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Client.Internal;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class ConfigurationRegistry : IConfigurationRegistry
    {
        private readonly ServiceManager _serviceManager;
        private List<Dictionary<string, string>> _configKeys = new List<Dictionary<string, string>>();
        private List<ConfigurationWatcher> _configWatchers = new List<ConfigurationWatcher>();

        internal ConfigurationRegistry(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public Task<bool> AddStaticKeyPathAsync(string keyPath)
        {
            keyPath = HttpUtils.StripFrontAndBackSlashes(keyPath);
            return AddInitialKeyPathAsync(keyPath).ContinueWith(r => r.Result > -1);
        }

        private async Task<int> AddInitialKeyPathAsync(string keyPath)
        {
            var response = await _serviceManager.Client.GetAsync($"{HttpUtils.KeyUrl}{keyPath}?recurse=true");
            if (!response.IsSuccessStatusCode)
            {
                return -1;
            }
            var content = await response.Content.ReadAsStringAsync();
            var keys = JsonConvert.DeserializeObject<KeyValue[]>(content);
            var dictionary = keys.ToDictionary(kv => kv.Key.Substring(keyPath.Length + 1).Replace('/', ':'), kv => kv.Value == null ? null : kv.ValueFromBase64(), StringComparer.OrdinalIgnoreCase);
            return AddNewDictionaryToList(dictionary);
        }

        public async Task AddUpdatingPathAsync(string keyPath)
        {
            keyPath = HttpUtils.StripFrontAndBackSlashes(keyPath);
            var intialDictionary = await AddInitialKeyPathAsync(keyPath);
            if (intialDictionary == -1)
            {
                var newDicitonary = new Dictionary<string, string>();
                intialDictionary = AddNewDictionaryToList(newDicitonary);
            }
            //We got values so lets start watching but we aren't waiting for this we will let it run
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            WatchingLoop(intialDictionary, keyPath);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task WatchingLoop(int indexOfDictionary, string keyPath)
        {
            try
            {
                var consulIndex = "0";
                string url = $"{HttpUtils.KeyUrl}{keyPath}?recurse=true&wait=300s&index=";
                while (true)
                {
                    var response = await _serviceManager.Client.GetAsync(url + consulIndex, _serviceManager.Cancelled);
                    consulIndex = response.GetConsulIndex();
                    if (!response.IsSuccessStatusCode)
                    {
                        //There is some error we need to do something 
                        continue;
                    }
                    var content = await response.Content.ReadAsStringAsync();
                    var keys = JsonConvert.DeserializeObject<KeyValue[]>(content);
                    var dictionary = keys.ToDictionary(kv => kv.Key.Substring(keyPath.Length + 1).Replace('/', ':'), kv => kv.Value == null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(kv.Value)), StringComparer.OrdinalIgnoreCase);
                    UpdateDictionaryInList(indexOfDictionary, dictionary);
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
                        Task.Run(watch.CallBack);
                    }
                    else
                    {
                        string newValue;
                        TryGetValue(watch.KeyToWatch, out newValue);
                        if (StringComparer.OrdinalIgnoreCase.Compare(watch.CurrentValue, newValue) != 0)
                        {
                            watch.CurrentValue = newValue;
                            Task.Run(watch.CallBack);
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
                _configWatchers.Add(new ConfigurationWatcher() { CallBack = callback });
            }
        }

        public void AddWatchOnSingleKey(string keyToWatch, Action callback)
        {
            lock (_configWatchers)
            {
                string currentValue;
                TryGetValue(keyToWatch, out currentValue);
                _configWatchers.Add(new ConfigurationWatcher() { CallBack = callback, KeyToWatch = keyToWatch, CurrentValue = currentValue });
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
