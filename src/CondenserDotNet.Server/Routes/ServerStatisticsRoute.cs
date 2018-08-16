using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace CondenserDotNet.Server.Routes
{
    public class ServerStatsRoute : ServiceBase
    {
        private readonly IRouteStore _store;

        public ServerStatsRoute(IRouteStore store) => _store = store;

        public override string[] Routes { get; } = { CondenserRoutes.Statistics };

        public override IPEndPoint IpEndPoint => throw new NotImplementedException();

        public override async Task CallService(HttpContext context)
        {
            var index = context.Request.Path.Value.LastIndexOf('/');
            if (index > 0)
            {
                var serviceName = context.Request.Path.Value.Substring(index + 1);
                var services = _store.GetServiceInstances(serviceName).ToArray();
                if (services.Any())
                {
                    var response = new ServerStats[services.Length];

                    for (var i = 0; i < services.Length; i++)
                    {
                        var service = services[i];

                        if (!(service is IUsageInfo usage)) continue;

                        double averageRequestTime = 0;
                        if (usage.Calls > 0)
                        {
                            averageRequestTime = usage.TotalRequestTime / usage.Calls;
                        }
                        response[i] = new ServerStats
                        {
                            ServiceId = service.ServiceId,
                            NodeId = service.NodeId,
                            Calls = usage.Calls,
                            AverageRequestTime = averageRequestTime,
                            LastRequest = usage.LastRequest,
                            LastRequestTime = usage.LastRequestTime,
                            Summary = service.GetSummary()
                        };
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.WriteJsonAsync(response);

                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync("Unknown server " + serviceName);

                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}
