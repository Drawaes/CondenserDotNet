using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using CondenserDotNet.Middleware.WindowsAuthentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace RoutingWithWindowsAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel((ops) =>
                {
                    ops.UseWindowsAuthentication();
                })
                .UseUrls($"http://*:{50000}")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
