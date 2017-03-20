namespace CondenserDotNet.Server.Routes
{
    public class HealthResponse
    {
        public CurrentState.Summary Stats { get; set; }
        public DataContracts.HealthCheck[] HealthChecks { get; set; }
    }
}
