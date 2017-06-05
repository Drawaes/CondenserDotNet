using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Middleware.TrailingHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ServiceRegistration
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddConsulServices();
        }

        public void Configure(IApplicationBuilder app, IServiceManager manager)
        {
            manager.AddHttpHealthCheck("health", 10).RegisterServiceAsync();
            var secondOps = Options.Create(new ServiceManagerConfig()
            {
                ServiceName = "TestService",
                ServicePort = 5000,
            });
            var manager2 = new ServiceManager(secondOps).AddHttpHealthCheck("health", 10).RegisterServiceAsync(); 
            app.UseMvcWithDefaultRoute();
        }
    }
}
