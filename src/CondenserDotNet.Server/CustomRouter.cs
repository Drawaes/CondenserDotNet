using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Server.Health;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class CustomRouter : IRouter
    {
        private readonly RoutingTrie.RadixTree<Service> _tree = new RoutingTrie.RadixTree<Service>();
        private static readonly Task _taskDone = Task.FromResult(0);
        private readonly IHealthRouter _healthRouter;
        private readonly ILogger<CustomRouter> _log;

        public CustomRouter(IHealthRouter healthRouter,
            ILoggerFactory factory)
        {
            _healthRouter = healthRouter;
            _log = factory?.CreateLogger<CustomRouter>();
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
        }

        public void AddServiceToRoute(string route, Service serviceToAdd)
        {
            _tree.AddServiceToRoute(route, serviceToAdd);
        }

        public void AddNewService(Service serviceToAdd)
        {
            foreach (var r in serviceToAdd.Routes)
            {
                _tree.AddServiceToRoute(r, serviceToAdd);
            }
        }

        public void RemoveService(Service serviceToRemove)
        {
            _tree.RemoveService(serviceToRemove);
        }

        public void RemoveServiceFromRoute(string route, Service serviceToRemove)
        {
            _tree.RemoveServiceFromRoute(route, serviceToRemove);
        }

        public Task RouteAsync(RouteContext context)
        {
            var path = context.HttpContext.Request.Path.Value;

            _log?.LogInformation("Route recieved for {path}", path);

            if (path == _healthRouter.Route)
            {
                context.Handler = _healthRouter.CheckHealth;
            }
            else
            {
                string matchedPath;
                Service s = _tree.GetServiceFromRoute(path, out matchedPath);
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

            }

            return _taskDone;
        }

        internal void CleanUpRoutes()
        {
            _log?.LogTrace("Compressing Trie Current Depth {maxDepth}", _tree.MaxDepth());
            _tree.Compress();
            _log?.LogTrace("Compressing Trie Finished New Depth {maxDepth}", _tree.MaxDepth());
        }
    }
}
