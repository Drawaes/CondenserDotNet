using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using CondenserDotNet.Server.WindowsAuthentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace RoutingWithWindowsAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new LoggerFactory();
            logger.AddConsole().AddDebug(LogLevel.Trace);

            var host = new WebHostBuilder()
                .UseKestrel((ops) =>
                {
                    ops.UseWindowsAuthentication();
                })
                .UseLoggerFactory(logger)
                .UseUrls($"http://*:{50000}")
                .AsCondenserRouter()
                .WithHealthRoute("/condenser/health")
                .WithHealthCheck(() => new HealthCheck
                {
                    Name = "Default",
                    Ok = true
                })
                .UsePreRouteMiddleware<WindowsAuthenticationMiddleware>()
                .Build();

            host.Run();
        }
    }
}
