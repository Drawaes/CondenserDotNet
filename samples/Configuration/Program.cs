using CondenserDotNet.Client;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Configuration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceManager = new ServiceManager("TestService");

            var setting = JsonConvert.SerializeObject(new ConsulConfig
            {
                Setting = "Test"
            });

            serviceManager.Config.SetKeyAsync("EVO/ConsulConfig", setting)
                .Wait();

            Startup.ServiceManager = serviceManager;

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{serviceManager.ServicePort}")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}