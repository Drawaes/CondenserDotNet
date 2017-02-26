using System.Collections.Generic;

namespace CondenserDotNet.Configuration.Consul
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