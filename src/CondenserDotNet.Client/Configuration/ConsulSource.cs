using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Client.Configuration
{
    public class ConsulSource : IConfigurationSource
    {
        private readonly IConfigurationRegistry _configurationRegistry;

        public ConsulSource(IConfigurationRegistry configurationRegistry)
        {
            _configurationRegistry = configurationRegistry;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulProvider(_configurationRegistry);
        }
    }
}