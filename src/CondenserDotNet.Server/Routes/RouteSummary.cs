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
        private readonly RoutingData _routingData;

        public RouteSummary(RoutingData routingData)
        {
            _routingData = routingData;
            Routes = new[] { CondenserRoutes.Summary };
        }

        public override string[] Routes { get; }
        public override bool RequiresAuthentication => true;
        public override IPEndPoint IpEndPoint => throw new NotImplementedException();

        public override Task CallService(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            object response = _routingData.ServicesWithHealthChecks
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