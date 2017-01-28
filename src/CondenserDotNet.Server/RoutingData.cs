using System.Collections.Generic;

namespace CondenserDotNet.Server
{
    public class RoutingData
    {
        public Dictionary<string, List<IService>> ServicesWithHealthChecks { get; } = new Dictionary<string, List<IService>>();

        public RoutingTrie.RadixTree<IService> Tree { get; } = new RoutingTrie.RadixTree<IService>();

    }
}