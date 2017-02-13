using System;
using CondenserDotNet.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceManager = new ServiceManager("TestService");

            //This setup would be done outside of this sample.  
            //The environment variable is passed to the startup to bootstrap
            var environment = "Org1";
            Environment
                .SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);

            var config = new ConsulConfig
            {
                Setting = "Test"
            };

            serviceManager.Config.SetKeyJsonAsync($"{environment}/ConsulConfig", config)
                .Wait();

            //End of set up

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{serviceManager.ServicePort}")
                .ConfigureServices(services =>
                {
                    services.AddSingleton(serviceManager);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}