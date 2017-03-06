using System;
using CondenserDotNet.Middleware.WindowsAuthentication;
using Microsoft.AspNetCore.Hosting;

namespace PocWebsocketsSupport
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args[0] == "pipe")
            {
                Startup.UsePipes = true;
            }
            var host = new WebHostBuilder()
               .UseKestrel( options =>
               {
                   //options.UseWindowsAuthentication();
               })
               .UseUrls($"*://10.0.76.1:50000")
               .UseStartup<Startup>()
               .Build();

            host.Run();
        }
    }
}