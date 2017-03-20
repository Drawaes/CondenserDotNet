using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
            var appLifetime = appBuilder.ApplicationServices.GetService<IApplicationLifetime>();
            var shutdownService = appBuilder.ApplicationServices.GetService<CleanShutdownService>();
            appLifetime.ApplicationStopping.Register(() => shutdownService.Shutdown());
            appBuilder.UseMiddleware<CleanShutdownMiddleware>();
            return appBuilder;
        }
    }
}
