using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CondenserDotNet.Services
{
    public interface ITtlCheck
    {
        Task<bool> ReportPassingAsync();
        Task<bool> ReportWarningAsync();
        Task<bool> ReportFailAsync();
        HealthCheck HealthCheck { get; }
    }
}
