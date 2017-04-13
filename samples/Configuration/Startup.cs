using CondenserDotNet.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration
{
    public class Startup
    {
        private readonly IConfigurationRegistry _registry;

        public Startup(IConfigurationRegistry registry)
        {
            _registry = registry;
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvcWithDefaultRoute();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureReloadable<ConsulConfig>(_registry);

            services.AddOptions()
                .AddRouting()
                .AddMvc();
        }
    }
}