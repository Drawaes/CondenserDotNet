using System.Collections.Generic;

namespace CondenserDotNet.Core.Routing
{
    public interface IRoutingStrategy<T>
    {
        T RouteTo(List<T> services);
        string Name { get; }
    }
}