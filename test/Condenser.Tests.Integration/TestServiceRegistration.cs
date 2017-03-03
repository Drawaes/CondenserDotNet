using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Condenser.Tests.Integration
{
    
    public class TestServiceRegistration
    {
        [Fact(Skip ="Not working on CI Builds")]
        public async Task RegisterAndUseHandler()
        {
            var resetEvent = new ManualResetEvent(false);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"*://*:{ServiceManagerConfig.GetNextAvailablePort()}")
                .ConfigureServices(collection => collection.AddSingleton(typeof(ManualResetEvent),resetEvent))
                .UseStartup<StartUp>()
                .Build();
            host.Start();

            resetEvent.WaitOne();

            //Create the registry and handler
            using (var registry = new ServiceRegistry(null))
            {
                var handler = registry.GetHttpHandler();
                var httpClient = new HttpClient(handler);
                var result = await httpClient.GetAsync("http://TestingHandler//This/Is/A/Test");
                Assert.True(result.IsSuccessStatusCode);
                var body = await result.Content.ReadAsStringAsync();
                Assert.Equal("it worked", body);
            }
            host.Dispose();
        }

        public class StartUp
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddConsulServices();
                services.AddOptions();
                services.Configure<ServiceManagerConfig>((ops) => ops.ServiceName = "TestingHandler");
            }

            public void Configure(IApplicationBuilder app, IServiceManager manager, ManualResetEvent resetEvent)
            {
                manager.AddHttpHealthCheck("/health", 1).AddApiUrl("/testSample/test3/test1").RegisterServiceAsync().Wait();
                app.Use(async (context,next) =>
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("it worked");
                    resetEvent.Set();
                });
            }
        }
    }
}
