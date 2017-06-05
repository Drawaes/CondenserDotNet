namespace CondenserDotNet.Configuration.Consul
{
    public class ConsulRegistryConfig
    {
        public string AgentAddress { get; set; } = "127.0.0.1";
        public int AgentPort { get; set; } = 8500;
        public IKeyParser KeyParser { get; set; } = new SimpleKeyValueParser();
    }
}
