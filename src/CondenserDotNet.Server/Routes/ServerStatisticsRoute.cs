using System;
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

        public override string[] Routes { get; } = { CondenserRoutes.Statistics };
        public override bool RequiresAuthentication => true;
        public override IPEndPoint IpEndPoint => throw new NotImplementedException();

        public override async Task CallService(HttpContext context)
        {
            var index = context.Request.Path.Value.LastIndexOf('/');
            if (index > 0)
            {
                var serviceName = context.Request.Path.Value.Substring(index + 1);
                if (_routingData.ServicesWithHealthChecks.TryGetValue(serviceName, out List<IService> services))
                {
                    var response = new ServerStats[services.Count];

                    for (var i = 0; i < services.Count; i++)
                    {
                        var service = services[i];
                        var usage = service as IUsageInfo;

                        if (usage == null) continue;

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
                            LastRequestTime = usage.LastRequestTime
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