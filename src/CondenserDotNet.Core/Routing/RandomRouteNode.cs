using System.Collections.Generic;

namespace CondenserDotNet.Core.Routing
{
    public class RandomRoutingStrategy<T> : IRoutingStrategy<T>
    {
        public static RandomRoutingStrategy<T> Default { get; } = new RandomRoutingStrategy<T>();

        public T RouteTo(List<T> instances) => instances?.Count > 0 ? instances[RandHelper.Next(0, instances.Count)] : default;
        public string Name { get; } = RouteStrategy.Random.ToString();
    }
}
