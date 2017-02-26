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
            var port = ServiceManagerConfig.GetNextAvailablePort();
            
            //This setup would be done outside of this sample.  
            //The environment variable is passed to the startup to bootstrap
            var environment = "Org1";
            Environment
                .SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
                       

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{port}")
                .ConfigureServices(services =>
                {
                    services.Configure<ServiceManagerConfig>(opts => opts.ServicePort = port);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}