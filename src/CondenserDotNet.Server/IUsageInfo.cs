using System;

namespace CondenserDotNet.Server
{
    public interface IUsageInfo 
    {
        int Calls { get; }
        double TotalRequestTime { get; }

        double LastRequestTime { get; }
        DateTime LastRequest { get; }
    }
}