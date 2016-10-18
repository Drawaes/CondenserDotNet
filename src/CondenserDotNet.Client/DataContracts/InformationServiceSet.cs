using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.DataContracts
{
    public class InformationServiceSet
    {
        public InformationNode Node { get;set;}
        public InformationService Service { get;set;}
        public InformationCheck[] Checks { get;set;}
    }
}
