namespace CondenserDotNet.Server.DataContracts
{
    public class HealthCheck
    {
        public bool Ok { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string CheckID { get; set; }
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string Node { get; set; }
        public HealthCheckStatus Status { get; set; }
    }
}