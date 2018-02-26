using System;
using System.Collections.Generic;
using System.Text;
using CondenserDotNet.Middleware.CleanShutdown;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Middleware.ConsulCleanShutdown
{
    public static class ConsulShutdownExtensions
    {
        public static IServiceCollection AddConsulShutdown(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ConsulShutdownService>();
            return serviceCollection.AddCleanShutdown();
        }

        public static IApplicationBuilder UseConsulShutdown(this IApplicationBuilder appBuilder, string shutdownMessage)
        {
            var appLifetime = appBuilder.ApplicationServices.GetService<IApplicationLifetime>();
            var shutdownService = appBuilder.ApplicationServices.GetService<ConsulShutdownService>();
            shutdownService.ShutdownMessage = shutdownMessage;
            appLifetime.ApplicationStopping.Register(shutdownService.Stopping);
            appLifetime.ApplicationStarted.Register(shutdownService.Started);
            return appBuilder.UseCleanShutdown();
        }
    }
}
