using CondenserDotNet.Middleware.CleanShutdown;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            var appLifetime = appBuilder.ApplicationServices.GetService<IHostApplicationLifetime>();
            var shutdownService = appBuilder.ApplicationServices.GetService<ConsulShutdownService>();
            shutdownService.ShutdownMessage = shutdownMessage;
            appLifetime.ApplicationStopping.Register(shutdownService.Stopping);
            appLifetime.ApplicationStarted.Register(shutdownService.Started);
            return appBuilder.UseCleanShutdown();
        }
    }
}
