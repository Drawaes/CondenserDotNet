using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.DataContracts
{
    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Session { get; set; }
        public string ValueFromBase64()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Value));
        }

        public bool IsDerivedKey { get; set; }
    }
}
