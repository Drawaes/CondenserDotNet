using System;
using System.Collections.Generic;
using System.Threading;

namespace CondenserDotNet.Core.Routing
{
    public class RandomRoutingStrategy<T> : IRoutingStrategy<T>
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> Random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public T RouteTo(List<T> instances)
        {
            if ((instances != null) && (instances.Count > 0))
                return instances[Random.Value.Next(0, instances.Count)];

            return default(T);
        }

        public string Name { get; } = RouteStrategy.Random.ToString();
    }
}