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
    public class ConsulConfigSource
    {
        private const string ConsulPath = "/";
        private const char ConsulPathChar = '/';
        private const char CorePath = ':';
        private const string ConsulKeyPath = "/v1/kv/";
        private const string IndexHeader = "X-Consul-Index";

        private readonly string _agentAddress;
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _disposed = new CancellationTokenSource();

        private IKeyParser _parser;

        public ConsulConfigSource(IOptions<ConsulRegistryConfig> agentConfig)
        {
            var agentInfo = agentConfig?.Value ?? new ConsulRegistryConfig();
            _agentAddress = $"http://{agentInfo.AgentAddress}:{agentInfo.AgentPort}";
            _parser = agentInfo.KeyParser;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_agentAddress)
            };
        }

        public string FormValidKey(string keyPath)
        {
            if (!keyPath.EndsWith(ConsulPath)) keyPath = keyPath + ConsulPath;

            return keyPath;

        }

        public async Task<(bool success, Dictionary<string, string> dictionary)> GetKeysAsync(string keyPath)
        {
            var response = await _httpClient.GetAsync($"{ConsulKeyPath}{keyPath}?recurse");
            if (!response.IsSuccessStatusCode)
            {
                return (false, null);
            }

            var dictionary = await BuildDictionaryAsync(keyPath, response);
            return (true, dictionary);
        }

        public async Task WatchKeysAsync(string keyPath, Action<Dictionary<string, string>> onUpdate)
        {
            var consulIndex = "0";
            var url = $"{ConsulKeyPath}{keyPath}?recurse&wait=300s&index=";
            try { 
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
                    onUpdate(dictionary);
                }
            }
            catch (TaskCanceledException) { /* nom nom */}
            catch (ObjectDisposedException) { /* nom nom */ }
        }

        public static string GetConsulIndex(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(IndexHeader, out IEnumerable<string> results))
            {
                return string.Empty;
            }
            return results.FirstOrDefault();
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

        public async Task<bool> TrySetKeyAsync(string keyPath, string value)
        {
            var response = await _httpClient.PutAsync($"{ConsulKeyPath}{keyPath}", GetStringContent(value));
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
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
    }
}
