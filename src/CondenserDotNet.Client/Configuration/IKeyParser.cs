using System.Collections.Generic;
using CondenserDotNet.Client.DataContracts;

namespace CondenserDotNet.Client.Configuration
{
    public interface IKeyParser
    {
        IEnumerable<KeyValue> Parse(KeyValue key);
    }
}