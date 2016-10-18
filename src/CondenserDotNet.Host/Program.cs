using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Host.RoutingTrie;
using Microsoft.AspNetCore.Hosting;

namespace CondenserDotNet.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
             new WebHostBuilder()
                .UseKestrel()
                .UseUrls(new[] { "http://0.0.0.0:5000/test/" })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
