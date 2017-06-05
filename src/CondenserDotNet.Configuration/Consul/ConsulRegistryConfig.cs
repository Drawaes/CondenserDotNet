namespace CondenserDotNet.Configuration.Consul
{
    public class ConsulRegistryConfig
    {
        public string AgentAddress { get; set; } = "localhost";
        public int AgentPort { get; set; } = 8500;
        public IKeyParser KeyParser { get; set; } = new SimpleKeyValueParser();
    }
}
