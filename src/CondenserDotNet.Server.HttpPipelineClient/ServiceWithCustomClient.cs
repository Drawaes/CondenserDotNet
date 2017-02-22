using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using Microsoft.AspNetCore.Http;

namespace CondenserDotNet.Server.HttpPipelineClient
{
    public class ServiceWithCustomClient : IService, IConsulService
    {
        private string _serviceId;
        private string _nodeId;
        private ConcurrentQueue<IPipeConnection> _pooledConnections = new ConcurrentQueue<IPipeConnection>();

        public ServiceWithCustomClient()
        {
            _pooledConnections = new ConcurrentQueue<IPipeConnection>();
        }

        public Version[] SupportedVersions => throw new NotImplementedException();
        public string[] Tags => throw new NotImplementedException();
        public string[] Routes => throw new NotImplementedException();
        public string ServiceId => _serviceId;
        public string NodeId => _nodeId;
        public IPEndPoint IpEndPoint => throw new NotImplementedException();
        public bool RequiresAuthentication => true;

        public async Task CallService(HttpContext context)
        {
            if(!_pooledConnections.TryDequeue(out IPipeConnection socket))
            {
                socket = await System.IO.Pipelines.Networking.Sockets.SocketConnection.ConnectAsync(IpEndPoint);
            }
            await socket.WriteHeadersAsync(context);
        }

        public Task Initialise(string serviceId, string nodeId, string[] tags, string address, int port)
        {
            throw new NotImplementedException();
        }

        public void UpdateRoutes(string[] routes)
        {
            throw new NotImplementedException();
        }
    }
}
