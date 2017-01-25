using System;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Configuration;
using CondenserDotNet.Server;
using CondenserDotNet.Service.DataContracts;
using CondenserTests.Fakes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CondenserTests
{
    public class TestServerFacts
    {
        [Fact]
        public async void CanReloadOptionDetails()
        {
            var registry = new FakeConfigurationRegistry();
            await registry.SetKeyAsync("FakeConfig:Setting1", "abc");
            await registry.SetKeyAsync("FakeConfig:Setting2", "def");

            var root = new ConfigurationBuilder()
                .AddJsonConsul(registry)
                .Build();

            var builder = new WebHostBuilder()
                .Configure(x => x.UseMvcWithDefaultRoute())
                .ConfigureServices(x =>
                {
                    x.AddMvcCore();
                    x.AddOptions();
                    x.ConfigureReloadable<FakeConfig>(root, registry);
                });

            using (var server = new TestServer(builder))
            {
                using (var client = server.CreateClient())
                {
                    var response = await client.GetAsync("Fake");
                    var setting = await response.Content.ReadAsStringAsync();

                    Assert.Equal("Config: abc", setting);

                    await registry.SetKeyAsync("FakeConfig:Setting1", "this is the new setting");
                    registry.FakeReload();

                    response = await client.GetAsync("Fake");
                    setting = await response.Content.ReadAsStringAsync();

                    Assert.Equal("Config: this is the new setting", setting);
                }
            }
        }

        [Fact]
        public async void CanRoutePath()
        {
            var apiBuilder = new WebHostBuilder()
                .Configure(x => x.UseMvcWithDefaultRoute())
                .ConfigureServices(x => { x.AddMvcCore(); });

            var customRouter = BuildRouter();
            var tags = new[] { "fake/fake/route" };
            var registry = new FakeServiceRegistry();
            var serviceId = "FakeService";

            var routerBuilder = new WebHostBuilder()
                .Configure(x =>
                {
                    x.UseRouter(customRouter);
                })
                .ConfigureServices(x =>
                {
                    x.AddRouting();
                    x.AddSingleton(customRouter);
                });

            using (var apiClient = new TestServer(apiBuilder))
            {
                var infoService = new InformationService
                {
                    Address = apiClient.BaseAddress.Host,
                    Port = apiClient.BaseAddress.Port,
                    Service = serviceId,
                    Tags = tags
                };

                registry.AddServiceInstance(infoService);

                //var serviceToAdd = new Service(tags,
                //serviceId, serviceId, tags,
                //registry, apiClient.CreateClient());

                //customRouter.AddNewService(serviceToAdd);
                
                using (var routerServer = new TestServer(routerBuilder))
                {
                    using (var routerClient = routerServer.CreateClient())
                    {
                        var response = await routerClient.GetAsync("fake/fake/route");
                        var setting = await response.Content.ReadAsStringAsync();

                        Assert.Equal("Was routed", setting);
                    }
                }
            }
        }

        private CustomRouter BuildRouter()
        {
            return new CustomRouter(new FakeHealthRouter());
        }
    }
}