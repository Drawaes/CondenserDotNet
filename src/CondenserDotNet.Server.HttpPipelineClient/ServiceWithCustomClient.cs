using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server.HttpPipelineClient
{
    public class ServiceWithCustomClient : IService, IConsulService
    {
        private readonly System.Threading.CountdownEvent _waitUntilRequestsAreFinished = new System.Threading.CountdownEvent(1);
        private string _address;
        private int _port;
        private byte[] _hostHeader;
        private CurrentState _stats;
        private IPEndPoint _ipEndPoint;
        private Version[] _supportedVersions;
        private string[] _tags;
        private string _serviceId;
        private ILogger _logger;
        private int _calls;
        private int _totalRequestTime;
        private ConcurrentQueue<IPipeConnection> _pooledConnections = new ConcurrentQueue<IPipeConnection>();
        private PipeFactory _factory;

        public ServiceWithCustomClient(CurrentState stats, ILoggerFactory loggingFactory, PipeFactory factory)
        {
            _logger = loggingFactory?.CreateLogger<ServiceWithCustomClient>();
            _stats = stats;
            _pooledConnections = new ConcurrentQueue<IPipeConnection>();
            _factory = factory;
        }

        public Version[] SupportedVersions => _supportedVersions;
        public string[] Tags => _tags;
        public string[] Routes { get; private set; }
        public string ServiceId => _serviceId;
        public string NodeId { get; private set; }
        public int Calls => _calls;
        public double TotalRequestTime => _totalRequestTime;
        public IPEndPoint IpEndPoint => _ipEndPoint;
        public bool RequiresAuthentication => true;

        public async Task CallService(HttpContext context)
        {
            try
            {
                if (!_pooledConnections.TryDequeue(out IPipeConnection socket))
                {
                    socket = await System.IO.Pipelines.Networking.Sockets.SocketConnection.ConnectAsync(IpEndPoint);
                    _logger?.LogInformation("Created new socket");
                }
                else
                {
                    _logger?.LogInformation("Got a connection from the pool, current pool size {poolSize}", _pooledConnections.Count);
                }
                await socket.WriteHeadersAsync(context, _hostHeader);
                await socket.WriteBodyAsync(context);
                await socket.ReceiveHeaderAsync(context);
                await socket.ReceiveBodyAsync(context, _logger);
                _pooledConnections.Enqueue(socket);
            }
            catch
            {
                throw new NotImplementedException();
            }
        }

        public override int GetHashCode()
        {
            return ServiceId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ServiceWithCustomClient otherService)
            {
                if (otherService.ServiceId != ServiceId)
                {
                    return false;
                }
                if (!Tags.SequenceEqual(otherService.Tags))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public void UpdateRoutes(string[] routes)
        {
            Routes = routes;
        }

        public async Task Initialise(string serviceId, string nodeId, string[] tags, string address, int port)
        {
            _address = address;
            _port = port;
            _tags = tags;
            Routes = ServiceUtils.RoutesFromTags(tags);
            _serviceId = serviceId;
            NodeId = nodeId;
            _hostHeader = Encoding.UTF8.GetBytes($"Host: {_address}:{_port}\r\n");
            try
            {
                var result = await Dns.GetHostAddressesAsync(address);
                _ipEndPoint = new IPEndPoint(result[0], port);
            }
            catch
            {
            }
            _supportedVersions = tags.Where(t => t.StartsWith("version=")).Select(t => new Version(t.Substring(8))).ToArray();
        }

        public void Dispose()
        {
            _waitUntilRequestsAreFinished.Signal();
            _waitUntilRequestsAreFinished.Wait(5000);
            _waitUntilRequestsAreFinished.Dispose();
        }
    }
}
