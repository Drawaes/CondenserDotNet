﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class ConfigurationManager
    {
        private readonly ServiceManager _serviceManager;
        private readonly List<Dictionary<string, string>> _configKeys = new List<Dictionary<string, string>>();

        internal ConfigurationManager(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<bool> AddStaticKeyPathAsync(string keyPath)
        {
            var response = await _serviceManager.Client.GetAsync($"{HttpUtils.KeyUrl}{keyPath}?recurse=true");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            var content = await response.Content.ReadAsStringAsync();
            var keys = JsonConvert.DeserializeObject<KeyValue[]>(content);
            _configKeys.Add(keys.ToDictionary(kv => kv.Key.Substring(keyPath.Length).Replace('/', ':'), kv => kv.Value == null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(kv.Value)), StringComparer.OrdinalIgnoreCase));

            return true;
        }

        //public async Task<bool> AddUpdatingPathAsync(string keyPath)
        //{

        //}

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

        public async Task<bool> SetKeyAsync(string keyPath, string value)
        {
            var response = await _serviceManager.Client.PutAsync($"{HttpUtils.KeyUrl}{keyPath}", HttpUtils.GetStringContent(value));
            if(!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
    }
}