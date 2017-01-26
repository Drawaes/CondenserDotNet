using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CondenserDotNet.Server.Health
{
    public class HealthRouter : IHealthRouter
    {
        private readonly IHealthConfig _config;
        private readonly CurrentState _state;
        
        public HealthRouter(IHealthConfig config, CurrentState stats)
        {
            _config = config;
            _state = stats;
        }

        public string Route => _config.Route;



        public async Task CheckHealth(HttpContext context)
        {
            var response = new HealthResponse();
            response.Stats = _state.GetSummary();

            if (_config.Checks?.Count > 0)
            {
                var checks = new Task<DataContracts.HealthCheck>[_config.Checks.Count];
                for (int i = 0; i < checks.Length; i++)
                {
                    checks[i] = _config.Checks[i]();
                }
                await Task.WhenAll(checks);
                var results = new DataContracts.HealthCheck[_config.Checks.Count];
                HttpStatusCode code = HttpStatusCode.OK;
                for (int i = 0; i < checks.Length; i++)
                {
                    results[i] = checks[i].Result;
                    if (!results[i].Ok)
                    {
                        code = HttpStatusCode.InternalServerError;
                    }
                }
                response.HealthChecks = results;
                context.Response.StatusCode = (int)code;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}