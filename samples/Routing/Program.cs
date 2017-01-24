using CondenserDotNet.Server.DataContracts;
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
                .AsCondenserRouter()
                    .WithHealthRoute("/condenser/health")
                    .WithHealthCheck(() => new HealthCheck
                    {
                        Name = "Default",
                        Ok = true
                    })
                .Build();

            host.Run();
        }
    }
}