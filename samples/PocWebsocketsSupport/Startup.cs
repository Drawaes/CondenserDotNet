using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.RoutingTrie;
using CondenserDotNet.Server.Websockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace PocWebsocketsSupport
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CondenserConfiguration>();
            services.AddTransient<Service>();
            services.AddSingleton<Func<IConsulService>>(x => x.GetService<Service>);
            services.AddTransient<IRoutingStrategy<IService>, RandomRoutingStrategy<IService>>();
            services.AddTransient<IRoutingStrategy<IService>, RoundRobinRoutingStrategy<IService>>();
            services.AddSingleton<IDefaultRouting<IService>, DefaultRouting<IService>>();

            Func<ChildContainer<IService>> factory = () =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>(new[] { randomRoutingStrategy }, null));
            };
            services.AddSingleton(new RoutingData(new RadixTree<IService>(factory)));
            services.AddSingleton<CustomRouter>();
            services.AddSingleton<RoutingHost>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<RoutingMiddleware>();
            app.UseMiddleware<WebsocketMiddleware>();
            app.Use(async (context, next) =>
            {
                await HandleRequest(context, next);
            });
        }

        private static async Task HandleRequest(HttpContext context, Func<Task> next)
        {
            var upgradeFeature = context.Features.Get<IHttpUpgradeFeature>();
            context.Request.Headers.Add("TextProxyHeader",new Microsoft.Extensions.Primitives.StringValues("Test"));
            if (upgradeFeature != null)
            {
                var securityKey = context.Request.Headers["Sec-WebSocket-Key"][0];
                
                var opaqueStream = await upgradeFeature.UpgradeAsync();
            }
                

            await next();
        }
    }
}
