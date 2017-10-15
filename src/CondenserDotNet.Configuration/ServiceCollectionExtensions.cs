using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CondenserDotNet.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self,
            IConfigurationRegistry registry)
            where TConfig : class => self.ConfigureReloadable<TConfig>(registry.Root, registry, typeof(TConfig).Name);

        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self,
            IConfigurationRegistry registry, string sectionName)
            where TConfig : class => self.ConfigureReloadable<TConfig>(registry.Root, registry, sectionName);


        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self,
            IConfiguration configuration, IConfigurationRegistry registry)
            where TConfig : class => self.ConfigureReloadable<TConfig>(configuration, registry, typeof(TConfig).Name);

        public static IServiceCollection ConfigureReloadable<TConfig>(this IServiceCollection self,
            IConfiguration configuration, IConfigurationRegistry registry, string sectionName)
            where TConfig : class
        {
            var initialised = false;
            self.Configure<TConfig>
            (config =>
            {
                void Bind()
                {
                    var section = configuration.GetSection(sectionName);
                    section.Bind(config);
                }

                if (!initialised)
                {
                    registry.AddWatchOnEntireConfig(Bind);
                    initialised = true;
                }
                Bind();
            });

            return self;
        }
    }
}
