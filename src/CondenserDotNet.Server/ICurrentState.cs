using CondenserDotNet.Server.DataContracts;
using System;

namespace CondenserDotNet.Server
{
    public interface ICurrentState
    {
        void RecordResponse(int responseCode);
        StatsSummary GetSummary();
        void ResetUptime();
    }   
}
