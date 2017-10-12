using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;

namespace CondenserDotNet.Server
{
    public interface IRouteSource
    {
        bool CanRequestRoute();
        Task<ServiceInstance[]> GetServiceInstancesAsync(string serviceName);
        Task<GetHealthCheckResult> TryGetHealthChecksAsync();
    }
}
