using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CondenserDotNet.Server.Routes
{
    public sealed class RouteSummary : ServiceBase
    {
        private readonly RoutingData _routingData;

        public RouteSummary(RoutingData routingData)
        {
            _routingData = routingData;
            Routes = new[] {"/admin/condenser/routes/summmary"};
        }

        public override string[] Routes { get; }

        public override Task CallService(HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.OK;

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
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}