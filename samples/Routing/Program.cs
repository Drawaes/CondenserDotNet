using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Routing
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{50000}")
                .AsCondenserRouter()
                .WithAgentAddress("docker")
                .WithHealthRoute("/condenser/health")
                .WithHealthCheck(() => new HealthCheck
                {
                    Name = "Default",
                    Ok = true
                })
                .UsePreRouteMiddleware<MyMiddleware>()
                .Build();

            host.Run();
        }
    }
}