using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Server.Routes
{
    public class HealthResponse
    {
        public CurrentState.ThreadStats Stats { get; set; }
        public DataContracts.HealthCheck[] HealthChecks { get; set; }
    }
}
