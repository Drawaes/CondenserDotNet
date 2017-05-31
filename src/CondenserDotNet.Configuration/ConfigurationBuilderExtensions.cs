using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddConfigurationRegistry(this IConfigurationBuilder self, IConfigurationRegistry registry)
        {
            var consul = new ConfigurationRegistrySource(registry);
            return self.Add(consul);
        }
    }
}