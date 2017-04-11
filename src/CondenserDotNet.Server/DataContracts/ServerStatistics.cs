using System;

namespace CondenserDotNet.Server.DataContracts
{
    public class ServerStats
    {
        public string ServiceId { get; set; }
        public string NodeId { get; set; }
        public int Calls { get; set; }
        public double AverageRequestTime { get; set; }

        public double LastRequestTime { get; set; }
        public DateTime LastRequest { get; set; }
        public StatsSummary Summary { get; set; }
    }
}