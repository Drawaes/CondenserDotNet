using System.Collections.Generic;
using CondenserDotNet.Client.DataContracts;

namespace CondenserDotNet.Client.Configuration
{
    internal class SimpleKeyValueParser : IKeyParser
    {
        public static readonly SimpleKeyValueParser Instance = new SimpleKeyValueParser();

        public IEnumerable<KeyValue> Parse(KeyValue key)
        {
            yield return key;
        }
    }
}