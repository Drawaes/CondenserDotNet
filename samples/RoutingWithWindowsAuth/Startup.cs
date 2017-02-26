using System;
using System.Collections.Generic;
using System.Text;
using CondenserDotNet.Server;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RoutingWithWindowsAuth
{
    public class Startup
    {
        public void Configure(ILoggerFactory logger)
        {
            logger.AddConsole().AddDebug(LogLevel.Trace);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCondenserWithBuilder()
                .WithHealthRoute("/condenser/health")
                .WithHealthCheck(() => new HealthCheck
                {
                    Name = "Default",
                    Ok = true
                }).Build();
        }
    }
}
