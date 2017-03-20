using System.Collections.Generic;

namespace CondenserDotNet.Configuration.Consul
{
    public interface IKeyParser
    {
        IEnumerable<KeyValue> Parse(KeyValue key);
    }
}
