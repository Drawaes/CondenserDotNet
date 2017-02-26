using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Configuration
{
    public class ConfigurationRegistrySource : IConfigurationSource
    {
        private readonly IConfigurationRegistry _configurationRegistry;

        public ConfigurationRegistrySource(IConfigurationRegistry configurationRegistry)
        {
            _configurationRegistry = configurationRegistry;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConfigurationRegistryProvider(_configurationRegistry);
        }
    }
}