using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.RoutingTrie;
using CondenserDotNet.Middleware.WindowsAuthentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CondenserDotNet.Middleware.Pipelines;

namespace PocWebsocketsSupport
{
    public class Startup
    {
        public static bool UsePipes { get; internal set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCondenser();
            services.AddSingleton(new PipeFactory());
            if (UsePipes)
            {
                services.AddTransient<ServiceWithCustomClient>();
                services.AddSingleton<Func<IConsulService>>(x => x.GetService<ServiceWithCustomClient>);
            }
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory logger)
        {
            //logger.AddConsole(LogLevel.Information, true);
            //app.UseWindowsAuthentication();
            app.UseCondenser();
        }
    }
}