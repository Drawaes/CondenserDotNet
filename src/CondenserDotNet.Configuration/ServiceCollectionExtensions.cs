using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Configuration
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self, IConfigurationRegistry registry)
            where TConfig : class =>
                self.ConfigureReloadable<TConfig>(registry, typeof(TConfig).Name);

        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self, IConfigurationRegistry registry, string sectionName)
            where TConfig : class
        {
            var initialised = false;
            self.Configure<TConfig>
            (config =>
            {
                Action bind = () =>
                {
                    var section = registry.Root.GetSection(sectionName);
                    section.Bind(config);
                };
                if (!initialised)
                {
                    registry.AddWatchOnEntireConfig(bind);
                    initialised = true;
                }
                bind();
            });

            return self;
        }
    }
}