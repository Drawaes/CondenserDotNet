using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.DataContracts
{
    public class Service
    {
        public string ID { get;set;}
        public string Name { get;set;}
        public string[] Tags { get;set;}
        public string Address { get;set;}
        public int Port { get;set;}
        public bool EnableTagOverride { get;set;}
        public HttpCheck Check { get;set;}

    }
}
