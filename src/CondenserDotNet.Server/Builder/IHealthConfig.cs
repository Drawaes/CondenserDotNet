using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;

namespace CondenserDotNet.Server.Builder
{
    public interface IHealthConfig
    {
        List<Func<Task<HealthCheck>>> Checks { get; }
        string Route { get; }
    }
}