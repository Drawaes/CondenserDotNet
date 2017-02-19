using System;
using CondenserDotNet.Client;
using Microsoft.AspNetCore.Hosting;

namespace WebsocketSampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceManager = new ServiceManager("TestServiceWebSocket", "localhost", 8500);

            serviceManager.ServicePort = 2222;
            serviceManager.AddHttpHealthCheck("/Health", 10)
                .AddApiUrl("/testsample/test3/test2");
            serviceManager.RegisterServiceAsync().Wait();
                        
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"*://*:{serviceManager.ServicePort}")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}