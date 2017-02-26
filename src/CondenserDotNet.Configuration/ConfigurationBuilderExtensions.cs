using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddConfigurationRegistry
            (this IConfigurationBuilder self, IConfigurationRegistry registry)
        {
            var consul = new ConfigurationRegistrySource(registry);
            self.Add(consul);
            return self;
        }
    }
}