using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CondenserDotNet.Configuration.Consul
{
    public class ConsulConfigSource : IConfigSource
    {
        private const string ConsulPath = "/";
        private const char CorePath = ':';
        private const string ConsulKeyPath = "/v1/kv/";

        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _disposed = new CancellationTokenSource();
        private readonly IKeyParser _parser;
        private readonly ILogger _logger;

        public ConsulConfigSource(IOptions<ConsulRegistryConfig> agentConfig, ILogger logger, IConsulAclProvider aclProvider)
        {
            _logger = logger;
            var agentInfo = agentConfig?.Value ?? new ConsulRegistryConfig();
            _parser = agentInfo.KeyParser;

            _httpClient = HttpUtils.CreateClient(aclProvider, agentInfo.AgentAddress, agentInfo.AgentPort);
        }

        private class WatchConsul
        {
            public string ConsulIndex { get; set; }
        }

        public object CreateWatchState() => new WatchConsul
        {
            ConsulIndex = "0"
        };

        public string FormValidKey(string keyPath)
        {
            if (!keyPath.EndsWith(ConsulPath))
            {
                keyPath = keyPath + ConsulPath;
            }
            if (keyPath.StartsWith(ConsulPath))
            {
                keyPath = keyPath.TrimStart(ConsulPath[0]);
            }
            return keyPath;
        }

        public async Task<KeyOperationResult> GetKeysAsync(string keyPath)
        {
            _logger?.LogTrace("Getting kv from path {ConsulKeyPath}{keyPath}", ConsulKeyPath, keyPath);
            try
            {
                var response = await _httpClient.GetAsync($"{ConsulKeyPath}{keyPath}?recurse");
                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("We didn't get a succesful response from consul code was {code}", response.StatusCode);
                    return new KeyOperationResult() { Success = false, Dictionary = null };
                }

                var dictionary = await BuildDictionaryAsync(keyPath, response);
                return new KeyOperationResult() { Success = true, Dictionary = dictionary };
            }
            catch (Exception ex)
            {
                _logger?.LogError(100, ex, "There was an exception getting the keys");
                throw;
            }
        }

        public async Task<(bool found, string value)> GetKeyAsync(string keyPath)
        {
            _logger?.LogTrace("Getting a single key from path {ConsulKeyPath}{keyPath}", ConsulKeyPath, keyPath);
            try
            {
                var response = await _httpClient.GetAsync($"{ConsulKeyPath}{keyPath}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("We didn't get a successful response from consul code was {code}", response.StatusCode);
                    return (false, null);
                }

                var content = await response.Content.ReadAsStringAsync();
                var keys = JsonConvert.DeserializeObject<KeyValue[]>(content);
                if (keys.Length != 1) return (false, null);
                return (true, keys[0].Value);
            }
            catch (Exception exception)
            {
                _logger?.LogError(100, exception, "There was an exception getting the keys");
                throw;
            }
        }

        public async Task<KeyOperationResult> TryWatchKeysAsync(string keyPath, object state)
        {
            _logger?.LogTrace("Starting to watch the keypath {keyPath}", keyPath);
            var consulState = (WatchConsul)state;
            var url = $"{ConsulKeyPath}{keyPath}?recurse&wait=300s&index=";
            try
            {
                var response = await _httpClient.GetAsync(url + consulState.ConsulIndex, _disposed.Token);
                var newConsulIndex = response.GetConsulIndex();

                if (!response.IsSuccessStatusCode)
                {
                    consulState.ConsulIndex = newConsulIndex;
                    return default;
                }

                if (newConsulIndex == consulState.ConsulIndex)
                {
                    consulState.ConsulIndex = newConsulIndex;
                    return default;
                }
                consulState.ConsulIndex = newConsulIndex;
                var dictionary = await BuildDictionaryAsync(keyPath, response);
                return new KeyOperationResult() { Success = true, Dictionary = dictionary };
            }
            catch (Exception ex)
            {
                _logger?.LogError(100, ex, "Error trying to watch a key {keyPath}", keyPath);
                throw;
            }
        }

        private class KeyValueComparer : IEqualityComparer<KeyValue>
        {
            private string _keyPath;
            private readonly char _consulPath;
            private readonly char _corePath;

            public KeyValueComparer(string keyPath, char consulPath, char corePath)
            {
                _keyPath = keyPath;
                _consulPath = consulPath;
                _corePath = corePath;
            }

            public bool Equals(KeyValue x, KeyValue y)
            {
                var xkey = x.Key.Substring(_keyPath.Length).Replace(_consulPath, _corePath);
                var yKey = y.Key.Substring(_keyPath.Length).Replace(_consulPath, _corePath);
                return xkey.Equals(yKey);
            }

            public int GetHashCode(KeyValue obj) => obj.Key.Substring(_keyPath.Length).Replace(_consulPath, _corePath).GetHashCode();
        }

        private async Task<Dictionary<string, string>> BuildDictionaryAsync(string keyPath,
            HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var keys = JsonConvert.DeserializeObject<KeyValue[]>(content);

            var parsedKeys = keys.SelectMany(k => _parser.Parse(k)).Distinct(new KeyValueComparer(keyPath, ConsulPath[0], CorePath));

            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in parsedKeys)
            {
                var key = kv.Key.Substring(keyPath.Length).Replace(ConsulPath[0], CorePath);
                var value = kv.IsDerivedKey ? kv.Value : kv.Value == null ? null : kv.ValueFromBase64();
                dictionary[key] = value;
            }
            return dictionary;
        }

        public async Task<bool> TrySetKeyAsync(string keyPath, string value)
        {
            var response = await _httpClient.PutAsync($"{ConsulKeyPath}{keyPath}", HttpUtils.GetStringContent(value));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
    }
}
