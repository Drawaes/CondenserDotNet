using System.Collections.Generic;
using System.Threading;

namespace CondenserDotNet.Core.Routing
{
    public class RoundRobinRoutingStrategy<T> : IRoutingStrategy<T>
    {
        private int _index = -1;

        public T RouteTo(List<T> instances)
        {
            if (instances?.Count > 0)
            {
                var index = Interlocked.Increment(ref _index);
                return instances[index % instances.Count];
            }
            return default;
        }

        public string Name { get; } = RouteStrategy.RoundRobin.ToString();
    }
}
