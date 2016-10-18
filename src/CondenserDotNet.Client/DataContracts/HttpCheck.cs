using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.DataContracts
{
    public class HttpCheck
    {
        public string HTTP { get;set;}
        public string Interval { get;set;}
        public string TTL { get;set;}
    }
}
