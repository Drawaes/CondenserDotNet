using CondenserDotNet.Configuration.Consul;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Configuration
{
    public class CondenserConfig
    {
        private CondenserConfig()
        {
        }

        public static CondenserConfig Begin()
        {
            return new CondenserConfig();
        }

        public CondenserConfig KeysStoredAsJson()
        {
            return this;
        }

        public IConfigurationRegistry Build()
        {
            var config = new ConsulRegistryConfig
            {
                
            };
            var options = Options.Create(config);
            return new ConsulRegistry(options);
        }
    }
}