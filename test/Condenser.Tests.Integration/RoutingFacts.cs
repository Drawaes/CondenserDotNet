using System;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Http;
using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Net;
using CondenserDotNet.Core;

namespace Condenser.Tests.Integration
{
    public class RoutingFacts
    {
        private const string UrlPrefix = "urlprefix-";

        [Fact]
        public async Task CanWeFindARouteAndGetAPage()
        {
            var informationService = new InformationService
            {
                Address = "www.google.com",
                Port = 80
            };

            var router = BuildRouter();
            var service = new Service(null, null, null);
            await service.Initialise("service1", "node1", new[] { UrlPrefix + "/search" }, "www.google.com", 80);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";

            var routedService = router.GetServiceFromRoute(context.Request.Path, out string matchedPath);
            await routedService.CallService(context);

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public async Task CanWeFindARouteAndGetAPageHttps()
        {
            var informationService = new InformationService
            {
                Address = "www.google.com",
                Port = 80
            };

            var router = BuildRouter();
            var service = new Service(null, null, null);
            await service.Initialise("service1", "node1", new[] { UrlPrefix + "/search", "protocolScheme-https" }, "www.google.com", 443);
            router.AddNewService(service);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/search";

            var routedService = router.GetServiceFromRoute(context.Request.Path, out string matchedPath);
            await routedService.CallService(context);
            Assert.Equal(200, context.Response.StatusCode);
        }

        private CustomRouter BuildRouter()
        {
            Func<ChildContainer<IService>> createNode = () =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy },
                    null));
            };
            var data = new RoutingData(new RadixTree<IService>(createNode));
            return new CustomRouter(null, data);
        }

        [Fact]
        public async Task CanWeRunRouter()
        {
            var hostPort = ServiceManagerConfig.GetNextAvailablePort();
            var routerPort = ServiceManagerConfig.GetNextAvailablePort();

            var hostService = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{hostPort}")
                .Configure(app =>
                {
                    app.Run(async x =>
                    {
                        if (x.Request.Path == "/Health")
                        {
                            x.Response.StatusCode = 200;
                            await x.Response.WriteAsync("healthy");
                        }
                        else
                        {
                            x.Response.StatusCode = 200;
                            await x.Response.WriteAsync("you called me");
                        }
                    });
                })
                .Build();

            RoutingHost host = null;

            var routerService = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{routerPort}")
                .ConfigureServices(x =>
                {
                    x.AddCondenser();                    
                })
                .Configure(app =>
                {
                    host = (RoutingHost)app.ApplicationServices
                        .GetService(typeof(RoutingHost));
                    app.UseCondenser();
                })
                .Build();

            var serviceName = "MyServiceToRoute";

            var options = Options.Create(new ServiceManagerConfig
            {
                ServiceName = serviceName,
                ServicePort = hostPort
                
            });

            var serviceManager = new ServiceManager(options);

            await serviceManager
                .AddHttpHealthCheck("/Health", 5)
                .AddApiUrl("/myservice")
                .RegisterServiceAsync();

            try
            {
                hostService.Start();
                routerService.Start();

                var waitForRegister = new AsyncManualResetEvent<bool>();                

                host.OnRouteBuilt = servers =>
                {
                    if (servers.ContainsKey(serviceName))
                    {
                        waitForRegister.Set(true);
                    }
                };

                await waitForRegister.WaitAsync();

                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };

                var response = await client.GetAsync($"http://localhost:{routerPort}/myservice");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("you called me", await response.Content.ReadAsStringAsync());

            }
            finally
            {
                hostService.Dispose();
                routerService.Dispose();
            }
        }

    }
}