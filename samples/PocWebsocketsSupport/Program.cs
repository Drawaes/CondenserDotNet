using System;
using CondenserDotNet.Server.WindowsAuthentication;
using Microsoft.AspNetCore.Hosting;

namespace PocWebsocketsSupport
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
               .UseKestrel( options =>
               {
                   options.UseWindowsAuthentication();
               })
               .UseUrls($"*://*:50000")
               .UseStartup<Startup>()
               .Build();

            host.Run();
        }
    }
}