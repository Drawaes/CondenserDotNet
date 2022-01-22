using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CondenserDotNet.Middleware.CleanShutdown
{
    public static class CleanShutdownExtensions
    {
        public static IServiceCollection AddCleanShutdown(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CleanShutdownService>();
            return serviceCollection;
        }

        public static IApplicationBuilder UseCleanShutdown(this IApplicationBuilder appBuilder)
        {
            var appLifetime = appBuilder.ApplicationServices.GetService<IHostApplicationLifetime>();
            var shutdownService = appBuilder.ApplicationServices.GetService<CleanShutdownService>();
            appLifetime.ApplicationStopping.Register(() => shutdownService.Shutdown());
            appBuilder.UseMiddleware<CleanShutdownMiddleware>();
            return appBuilder;
        }
    }
}
