using System.Collections.Generic;

namespace CondenserDotNet.Client.DataContracts
{
    public class Service
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool EnableTagOverride { get; set; }
        public List<HealthCheck> Checks { get; set; }
    }
}
