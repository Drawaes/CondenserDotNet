using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace CondenserDotNet.Server.Routes
{
    public sealed class ChangeRoutingStrategy : ServiceBase
    {
        private readonly IDefaultRouting<IService> _defaultRouting;
        private readonly IServiceProvider _provider;
        private readonly RoutingData _routingData;

        public ChangeRoutingStrategy(RoutingData routingData,
            IServiceProvider provider,
            IDefaultRouting<IService> defaultRouting)
        {
            _routingData = routingData;
            _provider = provider;
            _defaultRouting = defaultRouting;
        }

        public override string[] Routes => new string[]{ CondenserRoutes.Router };
        public override bool RequiresAuthentication => true;
        public override IPEndPoint IpEndPoint => throw new NotImplementedException();

        public override async Task CallService(HttpContext context)
        {
            var query = context.Request.QueryString;

            if (!query.HasValue)
            {
                await context.Response.WriteAsync("No query string args");
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return;
            }
            var queryDictionary = QueryHelpers.ParseQuery(query.Value);

            if (queryDictionary.TryGetValue("strategy", out StringValues values))
            {
                var router = _provider.GetServices<IRoutingStrategy<IService>>()
                    .SingleOrDefault(x => x.Name.Equals(values[0], StringComparison.OrdinalIgnoreCase));

                if (router != null)
                {
                    _defaultRouting.SetDefault(router);
                    ReplaceStrategy(_routingData.Tree.TopNode, router);
                    await context.Response.WriteAsync("Routing strategy has been replaced");
                }
            }
            else
            {
                await context.Response.WriteAsync("No query string args");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private void ReplaceStrategy(Node<IService> node, IRoutingStrategy<IService> strategy)
        {
            foreach (var child in node.ChildrenNodes)
            {
                child.Item2.Services.SetRoutingStrategy(strategy);
                ReplaceStrategy(child.Item2, strategy);
            }
        }
    }
}