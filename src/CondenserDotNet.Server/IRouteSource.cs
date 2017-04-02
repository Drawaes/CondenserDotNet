using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;

namespace CondenserDotNet.Server
{
    public interface IRouteSource
    {
        bool CanRequestRoute();
        Task<ServiceInstance[]> GetServiceInstances(string serviceName);
        Task<(bool success, HealthCheck[] checks)> TryGetHealthChecks();
    }
}