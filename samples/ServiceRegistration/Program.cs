using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Microsoft.AspNetCore.Hosting;

namespace ServiceRegistration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var port = ServiceManagerConfig.GetNextAvailablePort();
                                    
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{port}")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
