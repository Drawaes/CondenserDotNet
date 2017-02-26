using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CondenserDotNet.Server;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Routing
{
    public class Startup
    {
        public void Configure(ILoggerFactory logger)
        {
            logger.AddConsole(LogLevel.Trace);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCondenserWithBuilder()
                .WithHealthRoute("/condenser/health")
                .WithHealthCheck(() => new HealthCheck
                {
                    Name = "Default",
                    Ok = true
                })
                .WithHttpClient(serviceId => new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)

                }).Build();
        }
    }
}
