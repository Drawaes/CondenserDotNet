using System;
using System.Net.Http;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Filter;
using Microsoft.Extensions.Logging;

namespace Routing
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"*://*:{50000}")
                .UseStartup<Startup>()
                .Build();

            host.Run();

        }
    }
}