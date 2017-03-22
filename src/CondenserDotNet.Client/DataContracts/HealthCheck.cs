namespace CondenserDotNet.Client.DataContracts
{
    public class HealthCheck
    {
        public string HTTP { get; set; }
        public string Interval { get; set; }
        public string TTL { get; set; }
        public string Name { get; set; }
        public string DeregisterCriticalServiceAfter { get; set; }
        public bool tls_skip_verify { get; set; }
    }
}
