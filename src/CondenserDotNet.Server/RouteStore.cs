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

        public Dictionary<string, List<IService>> GetServices()
        {
            return _routingData.ServicesWithHealthChecks;
        }

        public List<IService> GetServiceInstances(string serviceName)
        {
            if(!_routingData.ServicesWithHealthChecks.TryGetValue(serviceName, out List<IService> services))
            {
                services = Empty;
            }

            return services;
        }

        public void RemoveService(string serviceName)
        {
            _routingData.ServicesWithHealthChecks.Remove(serviceName);
        }

        public bool HasService(string serviceName)
        {
            return _routingData.ServicesWithHealthChecks.ContainsKey(serviceName);
        }

        public void AddService(string serviceName)
        {
            _routingData.ServicesWithHealthChecks[serviceName] = new List<IService>();
        }

        public async Task<IService> CreateServiceInstance(ServiceInstance info)
        {
            var instance = _serviceFactory();

            if (!_routingData.Stats.TryGetValue(info.ServiceID, out ICurrentState stats))
            {
                stats = _statsFactory();
                _routingData.Stats.Add(info.ServiceID, stats);
            }
            else
            {
                stats.ResetUptime();
            }

            await instance.Initialise(info.ServiceID, info.Node, info.ServiceTags, info.ServiceAddress, info.ServicePort, stats);

            return instance;
        }

        public ICurrentState[] GetStats()
        {
           return _routingData.Stats.Values.ToArray();
        }

        public RadixTree<IService> GetTree()
        {
            return _routingData.Tree;
        }
    }
}
