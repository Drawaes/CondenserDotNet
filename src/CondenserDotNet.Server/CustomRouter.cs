using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CondenserDotNet.Server
{
    public class CustomRouter
    {
        private readonly RoutingData _routingData;
        private readonly ILogger<CustomRouter> _log;

        public CustomRouter(ILoggerFactory factory, RoutingData routingData, IEnumerable<IService> customRoutes)
        {
            _routingData = routingData;
            _log = factory?.CreateLogger<CustomRouter>();
            
            foreach (var customRoute in customRoutes)
            {
                AddNewService(customRoute);
            }
        }

        public IService GetServiceFromRoute(PathString path, out string matchedPath)
        {
            var service = _routingData.Tree.GetServiceFromRoute(path, out matchedPath);
            _log?.LogTrace("Looked for a service for the path {path} and got {service}", path, service);
            return service;
        }

        public void AddServiceToRoute(string route, IService serviceToAdd) => _routingData.Tree.AddServiceToRoute(route, serviceToAdd);
        
        public void AddNewService(IService serviceToAdd)
        {
            foreach (var r in serviceToAdd.Routes)
            {
                _routingData.Tree.AddServiceToRoute(r, serviceToAdd);
            }
        }

        public void RemoveService(IService serviceToRemove) => _routingData.Tree.RemoveService(serviceToRemove);

        public void RemoveServiceFromRoute(string route, IService serviceToRemove)
        {
            _log?.LogTrace("Removing {service} from {route}", serviceToRemove, route);
            _routingData.Tree.RemoveServiceFromRoute(route, serviceToRemove);
        }

        internal void CleanUpRoutes()
        {
            _log?.LogTrace("Compressing Trie Current Depth {maxDepth}", _routingData.Tree.MaxDepth());
            _routingData.Tree.Compress();
            _log?.LogTrace("Compressing Trie Finished New Depth {maxDepth}", _routingData.Tree.MaxDepth());
        }
    }
}
