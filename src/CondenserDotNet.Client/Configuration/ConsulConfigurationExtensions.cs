using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Client.Configuration
{
    public static class ConsulConfigurationExtensions
    {
        public static IConfigurationBuilder AddConsul
            (this IConfigurationBuilder self, IConfigurationRegistry registry)
        {
            var consul = new ConsulSource(registry);
            self.Add(consul);
            return self;
        }

        public static IConfigurationBuilder AddJsonConsul
            (this IConfigurationBuilder self, IConfigurationRegistry registry)
        {
            var consul = new ConsulSource(registry);
            registry.UpdateKeyParser(new JsonKeyValueParser());
            self.Add(consul);
            return self;
        }
    }
}