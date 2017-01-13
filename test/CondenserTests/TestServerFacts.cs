using CondenserDotNet.Client;
using CondenserTests.Fakes;
using Microsoft.Extensions.Configuration;
using CondenserDotNet.Client.Configuration;
using CondenserDotNet.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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


        [Fact(Skip = "Doesnt work yet")]
        public async void CanRoutePath()
        {
            var registry = new FakeConfigurationRegistry();
            await registry.SetKeyAsync("FakeConfig:Setting1", "abc");
            await registry.SetKeyAsync("FakeConfig:Setting2", "def");

            var config = new ConfigurationBuilder()
                .AddJsonConsul(registry)
                .Build();

            var apiBuilder = new WebHostBuilder()
                .Configure(x => x.UseMvcWithDefaultRoute())
                .ConfigureServices(x =>
                {
                    x.AddMvcCore();
                    x.AddOptions();
                    x.ConfigureReloadable<FakeConfig>(config, registry);
                });

            CustomRouter customRouter = null;
            var routerBuilder = new WebHostBuilder()
                .Configure(x =>
                {
                    customRouter = x.ApplicationServices.GetService<CustomRouter>();
                    x.UseRouter(customRouter);
                })
                .ConfigureServices(x =>
                {
                    x.AddRouting();
                    x.AddSingleton<CustomRouter>();
                });
            using (var apiServer = new TestServer(apiBuilder))
            {
                using (var routerServer = new TestServer(routerBuilder))
                {
                    using (var routerClient = routerServer.CreateClient())
                    {
                        var service = new Service(new string[0],
                            "Service1", "1", new string[0],
                            null);

                        customRouter.AddServiceToRoute("Fake", service);

                        var response = await routerClient.GetAsync("Fake");
                        var setting = await response.Content.ReadAsStringAsync();

                        Assert.Equal("Config: abc", setting);
                    }
                }
            }
        }
    }
}