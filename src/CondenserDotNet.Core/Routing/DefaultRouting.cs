using System.Collections.Generic;
using System.Linq;

namespace CondenserDotNet.Core.Routing
{
    public class DefaultRouting<T> : IDefaultRouting<T>
    {
        public DefaultRouting(IEnumerable<IRoutingStrategy<T>> strategy,
            IRoutingConfig config)
        {
            var name = (config?.DefaultRouteStrategy
                         ?? RouteStrategy.Random.ToString());

            Default = strategy.Single(x => x.Name == name);
        }

        public IRoutingStrategy<T> Default { get; private set; }
        public void SetDefault(IRoutingStrategy<T> strategy) => Default = strategy;
    }
}