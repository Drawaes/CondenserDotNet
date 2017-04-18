using System;
using CondenserDotNet.Client;
using CondenserDotNet.Configuration;
using CondenserDotNet.Configuration.Consul;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Configuration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var port = ServiceManagerConfig.GetNextAvailablePort();

            //This setup would be done outside of this sample.  
            //The environment variable is passed to the startup to bootstrap
            var environment = "Org1";
            Environment
                .SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
           
            var registry = CondenserConfigBuilder
                .FromConsul()
                .WithKeysStoredAsJson()
                .Build();

            //***Add some config
            var config = new ConsulConfig
            {
                Setting = "Test"
            };

            registry.SetKeyJsonAsync($"{environment}/ConsulConfig", config).Wait();
            //***


            registry.AddUpdatingPathAsync(environment).Wait();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{port}")
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IConfigurationRegistry>(registry);
                    services.Configure<ServiceManagerConfig>(opts => opts.ServicePort = port);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
