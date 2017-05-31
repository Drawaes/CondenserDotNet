using System;

namespace CondenserDotNet.Core.Routing
{
    public interface IRoutingConfig
    {
        string DefaultRouteStrategy { get; }
        Action<string[]> OnRoutesBuilt { get; }
    }
}
