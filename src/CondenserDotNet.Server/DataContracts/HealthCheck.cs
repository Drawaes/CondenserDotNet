namespace CondenserDotNet.Server.DataContracts
{
    public class HealthCheck
    {
        public bool Ok { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }
}