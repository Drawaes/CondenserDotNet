using CondenserDotNet.Server.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Routing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{50000}")
                .ConfigureServices(x => { x.AddCondenserRouter("localhost", 8500); })
                .Configure(x => { x.UseCondenserRouter(); })
                .Build();

            host.Run();
        }
    }
}