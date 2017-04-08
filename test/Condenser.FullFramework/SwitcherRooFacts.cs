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

        [Fact(Skip = "Full framework is broken on the switcher")]
        public async Task SwitcherooSeesHttpsFact()
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
                var client = new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, policy) =>
                    {
                        return false;
                    }
                });

                var result = await client.GetAsync($"https://localhost:{port}");
                var isHttp = await result.Content.ReadAsStringAsync();
                Assert.True(bool.Parse(isHttp));
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
                    await context.Response.WriteAsync(string.Join(",", Enumerable.Repeat("testingtesting", 100)));
                    return;
                });
        }
    }
}
