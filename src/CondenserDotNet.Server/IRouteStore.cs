using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.RoutingTrie;

namespace CondenserDotNet.Server
{
    public interface IRouteStore
    {
        void AddService(string serviceName);
        Task<IService> CreateServiceInstanceAsync(ServiceInstance info);
        Dictionary<string, List<IService>> GetServices();
        bool HasService(string serviceName);
        void RemoveService(string serviceName);
        ICurrentState[] GetStats();
        RadixTree<IService> GetTree();
        List<IService> GetServiceInstances(string serviceName);
    }
}