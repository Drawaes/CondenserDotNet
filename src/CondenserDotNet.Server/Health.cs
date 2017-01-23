using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CondenserDotNet.Server
{
    public class HealthRouter : IHealthRouter
    {
        private readonly IHealthConfig _config;

        public HealthRouter(IHealthConfig config)
        {
            _config = config;
        }

        public string Route => _config.Route;

        public async Task CheckHealth(HttpContext context)
        {
            if ((_config.Checks != null) && _config.Checks.Any())
            {
                var checks = _config.Checks.Select(x => x())
                    .ToArray();
                Task.WaitAll(checks);

                var healthChecks = checks
                    .Select(x => x.Result).ToArray();
                var result = JsonConvert.SerializeObject(healthChecks);

                var code = healthChecks.All(x => x.Ok)
                    ? (int) HttpStatusCode.OK
                    : (int) HttpStatusCode.InternalServerError;

                context.Response.StatusCode = code;
                await context.Response.WriteAsync(result);
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.Response.WriteAsync("Health was ok");
            }
        }
    }
}