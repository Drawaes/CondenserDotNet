using System;
using CondenserDotNet.Client;
using CondenserDotNet.Configuration;
using CondenserDotNet.Configuration.Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Configuration
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env
            , IConfigurationRegistry configRegistry, IServiceCollection services)
        {
            var config = new ConsulConfig
            {
                Setting = "Test"
            };

            configRegistry.SetKeyJsonAsync($"{env.EnvironmentName}/ConsulConfig", config).Wait();
            configRegistry.AddUpdatingPathAsync(env.EnvironmentName).Wait();

            var configBuilder = new ConfigurationBuilder()
                .AddConfigurationRegistry(configRegistry).Build();
            
            services.ConfigureReloadable<IConfigurationRegistry>(configBuilder, configRegistry);
            app.UseMvcWithDefaultRoute();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions()
                .AddSingleton<IConfigurationRegistry,ConsulRegistry>()
                .Configure<ConsulRegistryConfig>(ops => ops.KeyParser = new JsonKeyValueParser())
                .AddRouting()
                .AddMvc();
        }
    }
}