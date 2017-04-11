using System;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server.Routes
{

    public sealed class HealthRouter : ServiceBase
    {
        public HealthRouter() => Routes = new[] { CondenserRoutes.Health };

        public override string[] Routes { get; }
        public override IPEndPoint IpEndPoint => throw new NotImplementedException();

        public override async Task CallService(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.WriteJsonAsync("Ok");
        }
    }
}