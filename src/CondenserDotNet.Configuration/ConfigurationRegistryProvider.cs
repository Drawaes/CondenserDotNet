using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Configuration
{
    public class ConfigurationRegistryProvider : ConfigurationProvider
    {
        private readonly IConfigurationRegistry _configurationRegistry;

        public ConfigurationRegistryProvider(IConfigurationRegistry configurationRegistry)
        {
            _configurationRegistry = configurationRegistry;
            _configurationRegistry.AddWatchOnEntireConfig(Load);
        }

        public override bool TryGet(string key, out string value) => _configurationRegistry.TryGetValue(key, out value);
        public override void Set(string key, string value) => _configurationRegistry.SetKeyAsync(key, value).Wait();
        public override void Load() => OnReload();

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

            //Need to override this as we are not setting base Data, so expose all keys on registry
            return _configurationRegistry.AllKeys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(key => Segment(key, prefix.Length))
                .Concat(earlierKeys)
                .OrderBy(key => key, ConfigurationKeyComparer.Instance);
        }

        private static string Segment(string key, int prefixLength)
        {
            var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            return indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
        }
    }
}