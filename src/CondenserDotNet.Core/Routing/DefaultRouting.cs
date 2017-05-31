using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CondenserDotNet.Core.Routing
{
    public class DefaultRouting<T> : IDefaultRouting<T>
    {
        IRoutingStrategy<T> _default;
        public DefaultRouting(IEnumerable<IRoutingStrategy<T>> strategy, IRoutingConfig config)
        {
            var name = (config?.DefaultRouteStrategy ?? RouteStrategy.Random.ToString());
            SetDefault(strategy.Single(x => x.Name == name));
        }

        public IRoutingStrategy<T> Default => _default;
        public void SetDefault(IRoutingStrategy<T> strategy) => Interlocked.Exchange(ref _default, strategy);
    }
}