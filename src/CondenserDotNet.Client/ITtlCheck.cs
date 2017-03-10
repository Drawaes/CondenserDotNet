using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;

namespace CondenserDotNet.Client
{
    public interface ITtlCheck
    {
        Task<bool> ReportPassingAsync();
        Task<bool> ReportWarningAsync();
        Task<bool> ReportFailAsync();
        HealthCheck HealthCheck { get; }
    }
}