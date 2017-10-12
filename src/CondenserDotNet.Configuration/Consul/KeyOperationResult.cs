using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Configuration.Consul
{
    public struct KeyOperationResult
    {
        public bool Success;
        public Dictionary<string, string> Dictionary;
    }
}
