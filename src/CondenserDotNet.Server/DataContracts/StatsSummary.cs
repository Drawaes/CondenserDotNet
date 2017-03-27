using System;
using System.Collections.Generic;
using System.Text;
using CondenserDotNet.Server;

namespace CondenserDotNet.Server.DataContracts
{
    public struct StatsSummary 
    {
        public int Http100Responses { get; set; }
        public int Http200Responses { get; set; }
        public int Http300Responses { get; set; }
        public int Http400Responses { get; set; }
        public int Http500Responses { get; set; }
        public TimeSpan UpTime { get; set; }
        public int HttpUnknownResponse { get; set; }
    }
}
