using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server.Routes
{
    public sealed class RouteSummary : ServiceBase
    {
        private readonly IRouteStore _routeStore;

        public RouteSummary(IRouteStore routeStore)
        {
            _routeStore = routeStore;
            Routes = new[] { CondenserRoutes.Summary };
        }

        public override string[] Routes { get; }
        public override bool RequiresAuthentication => true;
        public override IPEndPoint IpEndPoint => throw new NotImplementedException();

        public override Task CallService(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            object response = _routeStore.GetServices()
                .Select(s => new
                {
                    Service = s.Key,
                    Nodes = s.Value
                        .Select(n => new
                        {
                            n.NodeId,
                            n.ServiceId,
                            n.SupportedVersions,
                            n.Routes,
                            n.Tags
                        })
                });
            return context.Response.WriteJsonAsync(response);
        }
    }
}