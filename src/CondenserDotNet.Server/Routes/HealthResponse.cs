using CondenserDotNet.Server.DataContracts;

namespace CondenserDotNet.Server.Routes
{
    public class HealthResponse
    {
        public StatsSummary Stats { get; set; }
        public HealthCheck[] HealthChecks { get; set; }
    }
}
