using CondenserDotNet.Middleware.WindowsAuthentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Condenser.Tests.Integration.Internal;

namespace Condenser.Tests.Integration
{
    public class AuthenticationMiddlewareFacts
    {
        [WindowsOnlyFact]
        public async Task CanAuthenticateWithNtlm()
        {
            var host = new WebHostBuilder()
                .UseKestrel((ops) =>
                {
                    ops.Listen(System.Net.IPAddress.Any, 55555, lo =>
                    {
                        lo.UseWindowsAuthentication();
                    });
                })
                .UseUrls($"http://*:{55555}")
                .UseStartup<Startup>()
                .Build();
            host.Start();

            try
            {
                var client = new HttpClient(new HttpClientHandler()
                {
                    UseDefaultCredentials = true
                });
                var result = await client.GetAsync($"http://localhost:55555");
                var name = await result.Content.ReadAsStringAsync();
                Assert.Equal(System.Security.Principal.WindowsIdentity.GetCurrent().Name, name);
            }
            finally
            {
                host.Dispose();
            }
        }

        public class Startup
        {
            public void Configure(IApplicationBuilder app)
            {
                app.UseMiddleware<WindowsAuthenticationMiddleware>();
                app.Use(async (context, next) =>
                {
                    await context.Response.WriteAsync(context.User.Identity.Name);
                    return;
                });
            }
        }

    }
}
