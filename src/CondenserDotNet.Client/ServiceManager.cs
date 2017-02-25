using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Core;

namespace CondenserDotNet.Client
{
    public class ServiceManager : IServiceManager
    {
        private readonly HttpClient _httpClient;
        private bool _disposed;
        private readonly string _serviceName;
        private readonly string _serviceId;
        private readonly List<string> _supportedUrls = new List<string>();
        private HealthCheck _httpCheck;
        private ITtlCheck _ttlCheck;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly ServiceRegistry _services;
        private readonly LeaderRegistry _leaders;
        private const int ConsulPort = 8500;
        private const string LocalHost = "localhost";

        public ServiceManager(string serviceName) : this(serviceName, $"{serviceName}:{Dns.GetHostName()}", LocalHost, ConsulPort) { }
        public ServiceManager(string serviceName, string serviceId) : this(serviceName, serviceId, LocalHost, ConsulPort) { }
        public ServiceManager(string serviceName, string agentAddress, int agentPort) : this(serviceName, $"{serviceName}:{Dns.GetHostName()}", agentAddress, agentPort) { }
        public ServiceManager(string serviceName, string serviceId, string agentAddress, int agentPort)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri($"http://{agentAddress}:{agentPort}") };
            _serviceId = serviceId;
            _serviceName = serviceName;
            _services = new ServiceRegistry(_httpClient, Cancelled);
            _leaders = new LeaderRegistry(this);
            ServiceAddress = Dns.GetHostName();
            ServicePort = GetNextAvailablePort();
        }

        public List<string> SupportedUrls => _supportedUrls;
        public HttpClient Client => _httpClient;
        public HealthCheck HttpCheck { get { return _httpCheck; } set { _httpCheck = value; } }
        public Service RegisteredService { get; set; }
        public string ServiceId => _serviceId;
        public string ServiceName => _serviceName;
        public TimeSpan DeregisterIfCriticalAfter { get; set; }
        public IServiceRegistry Services => _services;
        public bool IsRegistered => RegisteredService != null;
        public ITtlCheck TtlCheck { get { return _ttlCheck; } set { _ttlCheck = value; } }
        public string ServiceAddress { get; set; }
        public int ServicePort { get; set; }
        public CancellationToken Cancelled => _cancel.Token;
        public ILeaderRegistry Leaders => _leaders;

        protected int GetNextAvailablePort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            int port = 0;
            try
            {
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                l.Server.Dispose();
            }
            catch { /*Nom nom */}
            return port;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }
            try
            {
                _cancel.Cancel();
            }
            finally
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }

        ~ServiceManager()
        {
            Dispose(false);
        }
    }
}
