using CondenserDotNet.Client;
using CondenserDotNet.Client.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration
{
    public class Startup
    {
        public static ServiceManager ServiceManager;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonConsul(ServiceManager.Config);

            ServiceManager.Config.AddStaticKeyPathAsync("EVO").Wait();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddRouting();

            services.AddOptions();
            services.Configure<ConsulConfig>
                (Configuration.GetSection("ConsulConfig"));

            services.AddSingleton(Configuration);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvcWithDefaultRoute();
        }
    }
}