using System;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class CustomRouter 
    {
        private readonly RoutingData _routingData;
        private readonly ILogger<CustomRouter> _log;

        public CustomRouter(ILoggerFactory factory, RoutingData routingData)
        {
            _routingData = routingData;
            _log = factory?.CreateLogger<CustomRouter>();
        }
        
        public IService GetServiceFromRoute(PathString path, out string matchedPath)
        {
            return _routingData.Tree.GetServiceFromRoute(path, out matchedPath);
        }

        public void AddServiceToRoute(string route, IService serviceToAdd)
        {
            _routingData.Tree.AddServiceToRoute(route, serviceToAdd);
        }

        public void AddNewService(IService serviceToAdd)
        {
            foreach (var r in serviceToAdd.Routes)
            {
                _routingData.Tree.AddServiceToRoute(r, serviceToAdd);
            }
        }

        public void RemoveService(IService serviceToRemove)
        {
            _routingData.Tree.RemoveService(serviceToRemove);
        }

        public void RemoveServiceFromRoute(string route, IService serviceToRemove)
        {
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
