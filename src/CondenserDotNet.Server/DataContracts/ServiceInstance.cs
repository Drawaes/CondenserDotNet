using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Server.DataContracts
{
    public class ServiceInstance
    {
        public string Address { get; set; }
        public string ServiceID { get; set; }
        public string[] ServiceTags { get; set; }
        public string ServiceAddress { get; set; }
        public int ServicePort { get; set; }
    }
}
