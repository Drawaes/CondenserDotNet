using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CondenserDotNet.Configuration.Consul
{
    /// <summary>
    /// This manages the configuration keys you load as well as watching live keys and allowing you to add or update keys.
    /// </summary>
    public class ConsulRegistry : IConfigurationRegistry
    {
        private const string ConsulPath = "/";
        private const char ConsulPathChar = '/';
        private const char CorePath = ':';
        private const string ConsulKeyPath = "/v1/kv/";
        private const string IndexHeader = "X-Consul-Index";

        private readonly List<Dictionary<string, string>> _configKeys = new List<Dictionary<string, string>>();
        private readonly List<ConfigurationWatcher> _configWatchers = new List<ConfigurationWatcher>();
        private IKeyParser _parser;
        private readonly string _agentAddress;
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _disposed = new CancellationTokenSource();

        public ConsulRegistry(IOptions<ConsulRegistryConfig> agentConfig)
        {
            var agentInfo = agentConfig?.Value ?? new ConsulRegistryConfig();
            _agentAddress = $"http://{agentInfo.AgentAddress}:{agentInfo.AgentPort}";
            _parser = agentInfo.KeyParser;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_agentAddress)
            };
        }

        /// <summary>
        /// This returns a flattened list of all the loaded keys
        /// </summary>
        public IEnumerable<string> AllKeys => _configKeys.SelectMany(x => x.Keys);
        public void UpdateKeyParser(IKeyParser parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// This loads the keys from a path. They are not updated.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public Task<bool> AddStaticKeyPathAsync(string keyPath)
        {
            if (!keyPath.EndsWith(ConsulPath)) keyPath = keyPath + ConsulPath;
            return AddInitialKeyPathAsync(keyPath).ContinueWith(r => r.Result > -1);
        }

        private async Task<int> AddInitialKeyPathAsync(string keyPath)
        {
            var response = await _httpClient.GetAsync($"{ConsulKeyPath}{keyPath}?recurse");
            if (!response.IsSuccessStatusCode)
            {
                return -1;
            }

            var dictionary = await BuildDictionaryAsync(keyPath, response);

            return AddNewDictionaryToList(dictionary);
        }

        /// <summary>
        /// This loads the keys from a path. The path is then watched and updates are added into the configuration set
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public async Task AddUpdatingPathAsync(string keyPath)
        {
            if (!keyPath.EndsWith(ConsulPath)) keyPath = keyPath + ConsulPath;
            var initialDictionary = await AddInitialKeyPathAsync(keyPath);
            if (initialDictionary == -1)
            {
                var newDictionary = new Dictionary<string, string>();
                initialDictionary = AddNewDictionaryToList(newDictionary);
            }
            //We got values so lets start watching but we aren't waiting for this we will let it run
            var ignore = WatchingLoop(initialDictionary, keyPath);
        }

        private async Task WatchingLoop(int indexOfDictionary, string keyPath)
        {
            try
            {
                var consulIndex = "0";
                string url = $"{ConsulKeyPath}{keyPath}?recurse&wait=300s&index=";
                while (true)
                {
                    var response = await _httpClient.GetAsync(url + consulIndex, _disposed.Token);
                    consulIndex = GetConsulIndex(response);
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
                        TryGetValue(watch.KeyToWatch, out string newValue);
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
                if (TryGetValue(key, out string returnValue))
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
                    TryGetValue(keyToWatch, out string currentValue);
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
            var response = await _httpClient.PutAsync($"{ConsulKeyPath}{keyPath}", GetStringContent(value));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }

        public static string GetConsulIndex(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(IndexHeader, out IEnumerable<string> results))
            {
                return string.Empty;
            }
            return results.FirstOrDefault();
        }

        public static string StripFrontAndBackSlashes(string inputString)
        {
            int startIndex = inputString.StartsWith("/") ? 1 : 0;
            return inputString.Substring(startIndex, (inputString.Length - startIndex) - (inputString.EndsWith("/") ? 1 : 0));
        }

        public static StringContent GetStringContent(string stringForContent)
        {
            if (stringForContent == null)
            {
                return null;
            }
            var returnValue = new StringContent(stringForContent, Encoding.UTF8);
            return returnValue;
        }

        public void Dispose()
        {
            //Currently there is nothing to shutdown, but we should cleanup by writing some info
        }
    }
}
