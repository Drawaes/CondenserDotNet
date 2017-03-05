using System;
using CondenserDotNet.Middleware.ProtocolSwitcher;
using Microsoft.AspNetCore.Hosting;

namespace ProtocolSwitchingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Switcheroo();
                    options.UseHttps("testCert.pfx", "testPassword");
                })
                .UseUrls($"*://*:5000")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}