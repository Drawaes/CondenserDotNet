using System;
using System.Collections.Generic;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.RoutingTrie;

namespace CondenserDotNet.Server
{
    public class RoutingData
    {
        public RoutingData(RadixTree<IService> tree) => Tree = tree;

        public Dictionary<string, List<IService>> ServicesWithHealthChecks { get; } = new Dictionary<string, List<IService>>();
        public RadixTree<IService> Tree { get; }

        public Dictionary<string, ICurrentState> Stats { get; } = new Dictionary<string, ICurrentState>();

        public static RoutingData BuildDefault()
        {
            Func<ChildContainer<IService>> factory = () =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy }, null));
            };
            return new RoutingData(new RadixTree<IService>(factory));
        }
    }
}