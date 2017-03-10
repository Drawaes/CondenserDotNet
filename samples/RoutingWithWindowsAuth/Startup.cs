using System;
using System.Collections.Generic;
using System.Text;
using CondenserDotNet.Server;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace RoutingWithWindowsAuth
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory logger)
        {
            app.UseMiddleware<CondenserDotNet.Middleware.WindowsAuthentication.WindowsAuthenticationMiddleware>();
            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync(context.User.Identity.Name);
                return;
            });

            logger.AddConsole().AddDebug(LogLevel.Trace);

        }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCondenserWithBuilder()
            //    .WithHealthRoute("/condenser/health")
            //    .WithHealthCheck(() => new HealthCheck
            //    {
            //        Name = "Default",
            //        Ok = true
            //    }).Build();
        }
    }
}
