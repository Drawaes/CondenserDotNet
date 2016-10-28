using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.DataContracts
{
    public class FullKeyInfo
    {
        public string Session { get; set; }
        public string Value { get; set; }
        public int Flags { get; set; }
        public string Key { get; set; }
        public int LockIndex { get; set; }
        public int MofidyIndex { get; set; }
        public int CreateIndex { get; set; }
    }
}

