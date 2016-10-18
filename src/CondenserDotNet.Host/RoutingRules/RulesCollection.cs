using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Host.RoutingRules
{
    public class RulesCollection
    {
        public Dictionary<string, RoutingRule> RoutingRules { get;set;} = new Dictionary<string, RoutingRule>();

        
        
    }
}
