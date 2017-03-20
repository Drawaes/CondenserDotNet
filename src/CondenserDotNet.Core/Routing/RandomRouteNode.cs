using System.Collections.Generic;

namespace CondenserDotNet.Core.Routing
{
    public class RandomRoutingStrategy<T> : IRoutingStrategy<T>
    {
        public T RouteTo(List<T> instances)
        {
            if (instances?.Count > 0)
            {
                return instances[RandHelper.Next(0, instances.Count)];
            }
            return default(T);
        }

        public string Name { get; } = RouteStrategy.Random.ToString();
    }
}