using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Core.Consul
{
    public class AgentConfiguration
    {
        public string AgentAddress { get; set; } = "localhost";
        public int AgentPort { get; set; } = 8500;
    }
}
