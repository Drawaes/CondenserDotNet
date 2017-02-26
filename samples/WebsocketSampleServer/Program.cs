using System;
using CondenserDotNet.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace WebsocketSampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"*://*:2222")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}