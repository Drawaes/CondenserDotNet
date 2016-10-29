using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.DataContracts
{
    internal class HealthCheck
    {
        public string HTTP { get; set; }
        public string Interval { get; set; }
        public string TTL { get; set; }
        public string Name { get; set; }
    }
}
