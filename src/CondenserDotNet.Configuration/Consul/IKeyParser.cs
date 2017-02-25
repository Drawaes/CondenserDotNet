using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Configuration.Consul
{
    public interface IKeyParser
    {
        IEnumerable<KeyValue> Parse(KeyValue key);
    }
}
