using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Server.Extensions;
using CondenserDotNet.Server.RoutingTrie;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server.Routes
{
    public sealed class TreeRouter : ServiceBase
    {
        private readonly RoutingData _routingData;

        public TreeRouter(RoutingData routingData)
        {
            _routingData = routingData;
            Routes = new[] { CondenserRoutes.Tree };
        }

        public override string[] Routes { get; }
        public override IPEndPoint IpEndPoint => throw new NotImplementedException();
        
        public override Task CallService(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            var nodeDto = new Node();
            MapTo(_routingData.Tree.TopNode, nodeDto);
            return context.Response.WriteJsonAsync(nodeDto);
        }

        private void MapTo(Node<IService> node, Node dto)
        {
            dto.Path = node.Path;
            dto.Prefix = node.Prefix;
            dto.Services = node.Services.ToString();

            var children = new Dictionary<string, Node>(node.ChildrenNodes.Count);
            dto.Nodes = children;

            for (var i = 0; i < node.ChildrenNodes.Count; i++)
            {
                var nodeDto = new Node();
                var key = string.Join(",", node.ChildrenNodes.ElementAt(i).Item1);
                children.Add(key, nodeDto);
                MapTo(node.ChildrenNodes.ElementAt(i).Item2, nodeDto);
            }
        }
    }
}