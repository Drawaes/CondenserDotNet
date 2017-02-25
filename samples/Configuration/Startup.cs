using System;
using CondenserDotNet.Client;
using CondenserDotNet.Configuration;
using CondenserDotNet.Configuration.Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration
{
    public class Startup
    {
        private readonly ServiceManager _manager;

        public Startup(IHostingEnvironment env, ServiceManager manager)
        {
            _manager = manager;

            
            //manager.Config
            //    .AddUpdatingPathAsync(env.EnvironmentName)
            //    .Wait();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvcWithDefaultRoute();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IKeyParser>((sp) => new JsonKeyValueParser());
            services.AddSingleton<IConfigurationRegistry,ConsulRegistry>();
            
            services.AddMvc();
            services.AddRouting();

            services.AddOptions();
            //services.ConfigureReloadable<ConsulConfig>();

            //services.AddSingleton(Configuration);
            //return services.BuildServiceProvider();
        }
    }
}