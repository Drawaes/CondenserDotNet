using CondenserDotNet.Configuration.Consul;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Configuration
{
    public class CondenserConfigBuilder
    {
        private readonly ConsulRegistryConfig _config = new ConsulRegistryConfig();

        public CondenserConfigBuilder()
        {
        }

        public static CondenserConfigBuilder FromConsul() => new CondenserConfigBuilder();

        public CondenserConfigBuilder WithKeysStoredAsJson()
        {
            _config.KeyParser = new JsonKeyValueParser();
            return this;
        }

        public CondenserConfigBuilder WithAgentPort(int port)
        {
            _config.AgentPort = port;
            return this;
        }

        public CondenserConfigBuilder WithAgentAddress(string address)
        {
            _config.AgentAddress = address;
            return this;
        }


        public IConfigurationRegistry Build()
        {
            var options = Options.Create(_config);
            return new ConsulRegistry(options);
        }
    }
}
