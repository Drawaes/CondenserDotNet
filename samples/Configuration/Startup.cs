using System;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration
{
    public class Startup : IStartup
    {
        private readonly ServiceManager _manager;

        public Startup(IHostingEnvironment env,
            ServiceManager manager)
        {
            _manager = manager;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonConsul(manager.Config);

            manager.Config
                .AddUpdatingPathAsync(env.EnvironmentName)
                .Wait();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvcWithDefaultRoute();
        }

        IServiceProvider IStartup.ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddRouting();

            services.AddOptions();
            services.ConfigureReloadable<ConsulConfig>(Configuration, _manager.Config);

            services.AddSingleton(Configuration);
            return services.BuildServiceProvider();
        }
    }
}