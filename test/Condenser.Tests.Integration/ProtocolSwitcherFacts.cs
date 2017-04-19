using Microsoft.AspNetCore.Hosting;
using Microsoft.DotNet.PlatformAbstractions;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;
using CondenserDotNet.Middleware.ProtocolSwitcher;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Condenser.Tests.Integration
{
    public class ProtocolSwitcherFacts
    {
        public static X509Certificate2 Certificate = new X509Certificate2(Path.Combine(ApplicationEnvironment.ApplicationBasePath, @"TestCert.pfx"), "Test123t");
        
        [Fact]
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
                        return true;
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

        [Fact]
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
                var client = new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, policy) =>
                    {
                        return false;
                    }
                });

                var result = await client.GetAsync($"http://localhost:{port}");
                var isHttp = await result.Content.ReadAsStringAsync();
                Assert.False(bool.Parse(isHttp));
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
