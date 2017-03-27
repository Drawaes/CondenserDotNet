using System;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace CondenserDotNet.Server.Routes
{
    public sealed class HealthRouter : ServiceBase
    {
        private readonly IHealthConfig _config;
        private readonly RoutingData _data;

        public HealthRouter(IHealthConfig config, RoutingData data)
        {
            _config = config;
            Routes = new[] { _config.Route };
            _data = data;
        }

        public override string[] Routes { get; }
        public override IPEndPoint IpEndPoint => throw new NotImplementedException();
        public override bool RequiresAuthentication => false;

        public override async Task CallService(HttpContext context)
        {
           var summary = GetAggregatedSummary();
           var response = new HealthResponse { Stats = summary };

            if (_config.Checks?.Count > 0)
            {
                var checks = new Task<HealthCheck>[_config.Checks.Count];
                for (var i = 0; i < checks.Length; i++)
                    checks[i] = _config.Checks[i]();
                await Task.WhenAll(checks);
                var results = new HealthCheck[_config.Checks.Count];
                var code = HttpStatusCode.OK;
                for (var i = 0; i < checks.Length; i++)
                {
                    results[i] = checks[i].Result;
                    if (!results[i].Ok)
                        code = HttpStatusCode.InternalServerError;
                }
                response.HealthChecks = results;
                context.Response.StatusCode = (int)code;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            await context.Response.WriteJsonAsync(response);
        }

        private CurrentState.Summary GetAggregatedSummary()
        {
            var services = _data.ServicesWithHealthChecks
                .SelectMany(s => s.Value)
                .ToArray();

            var summary = new CurrentState.Summary();

            foreach(var service in services)
            {
                var stats = service.GetSummary();

                summary.Http100Responses += stats.Http100Responses;
                summary.Http200Responses += stats.Http200Responses;
                summary.Http300Responses += stats.Http300Responses;
                summary.Http400Responses += stats.Http400Responses;
                summary.Http500Responses += stats.Http500Responses;
                summary.HttpUnknownResponse += stats.HttpUnknownResponse;
            }

            return summary;
        }
    }
}