using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self,
            IConfiguration configuration, IConfigurationRegistry registry)
            where TConfig : class
        {
            return self.ConfigureReloadable<TConfig>(configuration, registry, typeof(TConfig).Name);
        }

        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self,
            IConfiguration configuration, IConfigurationRegistry registry,
            string sectionName)
            where TConfig : class
        {
            var initialised = false;
            self.Configure<TConfig>
            (config =>
            {
                Action bind = () =>
                {
                    var section = configuration.GetSection(sectionName);
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