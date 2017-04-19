using CondenserDotNet.Middleware.ProtocolSwitcher;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace Condenser.FullFramework
{
    public class SwitcherRooFacts
    {
        public static X509Certificate2 Certificate = new X509Certificate2(@"TestCert.pfx", "Test123t");

        [Fact(Skip ="")]
        public async Task SwitcherooSeesHttpsFact()
        {
            var port = CondenserDotNet.Client.ServiceManagerConfig.GetNextAvailablePort();
            var host = new WebHostBuilder()
                .UseKestrel((ops) =>
                {
                    //ops.Switcheroo();
                    ops.UseHttps(Certificate);
                })
                .UseUrls($"https://*:{port}")
                .UseStartup<Startup>()
                .Build();
            host.Start();

            var t = Task.Run(() =>
            {
                try
                {
                    var client = new HttpClient(new HttpClientHandler()
                    {
                    });

                    var result = client.GetAsync($"https://localhost:{port}");
                    result.Wait();
                    var isHttps = result.Result.Content.ReadAsStringAsync();
                    isHttps.Wait();
                    Assert.True(bool.Parse(isHttps.Result));
                }
                finally
                {
                    host.Dispose();
                }
            });
            await t.ConfigureAwait(false);
        }

        [Fact(Skip ="")]
        public async Task SwitcherooSeesHttpFact()
        {
            var port = CondenserDotNet.Client.ServiceManagerConfig.GetNextAvailablePort();
            var host = new WebHostBuilder()
                .UseKestrel((ops) =>
                {
                    ops.Switcheroo();
                    ops.UseHttps(Certificate);
                })
                .UseUrls($"*://*:{port}")
                .UseStartup<Startup>()
                .Build();
            host.Start();

            try
            {
                var client = new HttpClient();

                var result = await client.GetAsync($"http://localhost:{port}");
                var isHttps = await result.Content.ReadAsStringAsync();
                Assert.False(bool.Parse(isHttps));
            }
            finally
            {
                host.Dispose();
            }
        }

        public class Startup
        {
            public void Configure(IApplicationBuilder app) =>
                app.Use(async (context, next) =>
                {
                    await context.Response.WriteAsync(context.Request.IsHttps.ToString());
                    return;
                });
        }
    }
}
