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
            var logger = new LoggerFactory();
            logger.AddConsole().AddDebug(LogLevel.Trace);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseLoggerFactory(logger)
                .UseUrls($"http://*:{50000}")
                .AsCondenserRouter()
                .WithHealthRoute("/condenser/health")
                .WithHealthCheck(() => new HealthCheck
                {
                    Name = "Default",
                    Ok = true
                })
                .UsePreRouteMiddleware<CondenserDotNet.Server.Authentication.WindowsAuthenticationMiddleware>()
                .Build();

            host.Run();
        }
    }
}