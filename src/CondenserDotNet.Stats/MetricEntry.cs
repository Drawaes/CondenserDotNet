using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.StatsD
{
    public struct MetricEntry
    {
        public string MetricName { get; set; }
        public long Value { get; set; }
        public MetricType Type { get; set; }
        public DateTime MetricTime { get; set; }

        public long UnixTime => (MetricTime - DateTime.UtcNow).Ticks * 100;
    }
}
