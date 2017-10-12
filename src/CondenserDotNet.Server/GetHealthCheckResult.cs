using System;
using System.Collections.Generic;
using System.Text;
using CondenserDotNet.Server.DataContracts;

namespace CondenserDotNet.Server
{
    public struct GetHealthCheckResult
    {
        public bool Success;
        public HealthCheck[] Checks;
    }
}
