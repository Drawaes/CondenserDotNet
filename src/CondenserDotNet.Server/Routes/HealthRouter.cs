﻿using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.Builder;
using CondenserDotNet.Server.DataContracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CondenserDotNet.Server.Routes
{
    public sealed class HealthRouter : ServiceBase
    {
        private readonly IHealthConfig _config;
        private readonly CurrentState _state;

        public HealthRouter(IHealthConfig config, CurrentState stats)
        {
            _config = config;
            _state = stats;

            Routes = new[] {_config.Route};
        }

        public override string[] Routes { get; }

        public override async Task CallService(HttpContext context)
        {
            var response = new HealthResponse {Stats = _state?.GetSummary()};

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
                context.Response.StatusCode = (int) code;
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.OK;
            }
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}