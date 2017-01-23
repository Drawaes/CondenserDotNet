using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace CondenserDotNet.Server
{
    public class CustomRouter : IRouter
    {
        private readonly RoutingTrie.RadixTree<Service> _tree = new RoutingTrie.RadixTree<Service>();
        private static readonly Task _taskDone = Task.FromResult(0);
        private readonly IHealthRouter _healthRouter;

        public CustomRouter(IHealthRouter healthRouter)
        {
            _healthRouter = healthRouter;
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
            foreach(var r in serviceToAdd.Routes)
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
                    context.Handler = s.CallService;
                }
                
            }

            return _taskDone;
        }

        internal void CleanUpRoutes()
        {
            _tree.Compress();
        }
    }
}
