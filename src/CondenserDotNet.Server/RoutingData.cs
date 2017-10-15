using System;
using System.Collections.Generic;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.RoutingTrie;
using System.Linq;

namespace CondenserDotNet.Server
{
    public class RoutingData
    {
        private Dictionary<string, ICurrentState> _stats = new Dictionary<string, ICurrentState>();

        public RoutingData(RadixTree<IService> tree) => Tree = tree;

        public Dictionary<string, List<IService>> ServicesWithHealthChecks { get; } = new Dictionary<string, List<IService>>();
        public RadixTree<IService> Tree { get; }

        public static RoutingData BuildDefault()
        {
            ChildContainer<IService> factory()
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy }, null));
            }
            return new RoutingData(new RadixTree<IService>(factory));
        }

        public ICurrentState GetStats(string serviceId)
        {
            lock(_stats)
            {
                if(!_stats.TryGetValue(serviceId,out var value))
                {
                    value = new CurrentState();
                    _stats.Add(serviceId, value);
                }
                return value;
            }
        }

        public ICurrentState[] GetAllStats()
        {
            lock(_stats)
            {
                return _stats.Values.ToArray();
            }
        }
    }
}
