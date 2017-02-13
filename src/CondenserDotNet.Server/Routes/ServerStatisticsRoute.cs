using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server.Routes
{
    public class ServerStatsRoute : ServiceBase
    {
        private readonly RoutingData _routingData;

        public ServerStatsRoute(RoutingData routingData)
        {
            _routingData = routingData;
        }

        public override string[] Routes { get; } = {CondenserRoutes.Statistics};

        public override async Task CallService(HttpContext context)
        {
            var index = context.Request.Path.Value.LastIndexOf('/');

            if (index > 0)
            {
                var serviceName = context.Request.Path.Value.Substring(index + 1);

                List<IService> services;

                if (_routingData.ServicesWithHealthChecks.TryGetValue(serviceName, out services))
                {
                    var response = new ServerStats[services.Count];

                    for (var i = 0; i < services.Count; i++)
                    {
                        var service = services[i];
                        var usage = service as IUsageInfo;

                        if (usage == null) continue;

                        double averageRequestTime = 0;
                        if (usage.Calls > 0)
                            averageRequestTime = usage.TotalRequestTime / usage.Calls;

                        response[i] = new ServerStats
                        {
                            ServiceId = service.ServiceId,
                            NodeId = service.NodeId,
                            Calls = usage.Calls,
                            AverageRequestTime = averageRequestTime
                        };
                    }

                    await context.Response.WriteJsonAsync(response);
                    context.Response.StatusCode = (int) HttpStatusCode.OK;
                }
                else
                {
                    await context.Response.WriteAsync("Unknown server " + serviceName);
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}