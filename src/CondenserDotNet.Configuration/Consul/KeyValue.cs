using System;
using System.Text;

namespace CondenserDotNet.Configuration.Consul
{
    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Session { get; set; }
        public bool IsDerivedKey { get; set; }

        public string ValueFromBase64() => Encoding.UTF8.GetString(Convert.FromBase64String(Value));
    }
}
