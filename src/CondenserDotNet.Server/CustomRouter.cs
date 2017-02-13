using System;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class CustomRouter : IRouter
    {
        private readonly RoutingData _routingData;
        private static readonly Task _taskDone = Task.FromResult(0);
        private readonly ILogger<CustomRouter> _log;

        public CustomRouter(ILoggerFactory factory, RoutingData routingData)
        {
            _routingData = routingData;
            _log = factory?.CreateLogger<CustomRouter>();
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
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

        public Task RouteAsync(RouteContext context)
        {
            var path = context.HttpContext.Request.Path.Value;

            _log?.LogInformation("Route recieved for {path}", path);

            string matchedPath;
            var s = _routingData.Tree.GetServiceFromRoute(path, out matchedPath);
            context.RouteData.DataTokens.Add("apiPath", matchedPath);
            if (s != null)
            {
                _log?.LogInformation("Routing through service {s.ServiceId}", s.ServiceId);
                context.Handler = s.CallService;
            }
            else
            {
                _log?.LogInformation("No route recieved for {path}", path);
            }


            return _taskDone;
        }

        internal void CleanUpRoutes()
        {
            _log?.LogTrace("Compressing Trie Current Depth {maxDepth}", _routingData.Tree.MaxDepth());
            _routingData.Tree.Compress();
            _log?.LogTrace("Compressing Trie Finished New Depth {maxDepth}", _routingData.Tree.MaxDepth());
        }
    }
}
