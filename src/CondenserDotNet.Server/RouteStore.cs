using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using System.Linq;
using CondenserDotNet.Server.RoutingTrie;

namespace CondenserDotNet.Server
{
    public class RouteStore : IRouteStore
    {
        private readonly RoutingData _routingData;
        private readonly Func<IConsulService> _serviceFactory;
        private readonly Func<ICurrentState> _statsFactory;
        readonly static List<IService> Empty = new List<IService>();

        public RouteStore(RoutingData routingData, Func<IConsulService> serviceFactory,
            Func<ICurrentState> statsFactory)
        {
            _routingData = routingData;
            _serviceFactory = serviceFactory;
            _statsFactory = statsFactory;
        }

        public void RemoveService(string serviceName) => _routingData.ServicesWithHealthChecks.Remove(serviceName);
        public bool HasService(string serviceName) => _routingData.ServicesWithHealthChecks.ContainsKey(serviceName);
        public void AddService(string serviceName) => _routingData.ServicesWithHealthChecks[serviceName] = new List<IService>();
        public Dictionary<string, List<IService>> GetServices() => _routingData.ServicesWithHealthChecks;
        public ICurrentState[] GetStats() => _routingData.GetAllStats();
        public RadixTree<IService> GetTree() => _routingData.Tree;

        public List<IService> GetServiceInstances(string serviceName)
        {
            if (!_routingData.ServicesWithHealthChecks.TryGetValue(serviceName, out var services))
            {
                services = Empty;
            }

            return services;
        }
                
        public Task<IService> CreateServiceInstanceAsync(ServiceInstance info)
        {
            var instance = _serviceFactory();
            return instance.Initialise(info.ServiceID, info.Node, info.ServiceTags, info.ServiceAddress, info.ServicePort)
                .ContinueWith(t => (IService)instance);
        }
    }
}
