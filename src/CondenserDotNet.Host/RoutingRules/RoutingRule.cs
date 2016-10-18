using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Host.RoutingRules
{
    public class RoutingRule
    {
        public string RuleName { get;set;}
        public Dictionary<string, double> WeightsByTags { get;set;} = new Dictionary<string, double>();

        public Dictionary<string, string> TagToUserCategory { get;set;} = new Dictionary<string, string>();
    }
}
