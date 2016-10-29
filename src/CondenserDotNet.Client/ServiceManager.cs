using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    public class ServiceManager : IDisposable
    {
        private HttpClient _httpClient;
        private bool _disposed = false;
        private string _serviceName;
        private string _serviceId;
        private bool _isRegistered;
        private List<string> _supportedUrls = new List<string>();
        private HealthCheck _httpCheck;
        private TtlCheck _ttlCheck;
        private CountdownEvent _shutdownCounter = new CountdownEvent(0);
        private readonly ConfigurationManager _config;

        public ServiceManager(string serviceName) : this(serviceName, $"{serviceName}:{Dns.GetHostName()}", "localhost", 8500) { }
        public ServiceManager(string serviceName, string serviceId) : this(serviceName, serviceId, "localhost", 8500) { }
        public ServiceManager(string serviceName, string agentAddress, int agentPort) : this(serviceName, $"{serviceName}:{Dns.GetHostName()}", agentAddress, agentPort) { }
        public ServiceManager(string serviceName, string serviceId, string agentAddress, int agentPort)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"http://{agentAddress}:{agentPort}");
            _serviceId = serviceId;
            _serviceName = serviceName;
            _config = new ConfigurationManager(this);
            ServiceAddress = Dns.GetHostName();
            ServicePort = GetNextAvailablePort();
        }

        internal List<string> SupportedUrls => _supportedUrls;
        public ConfigurationManager Config => _config;
        internal HttpClient Client => _httpClient;
        internal HealthCheck HttpCheck { get { return _httpCheck; } set { _httpCheck = value; } }
        public string ServiceAddress { get; set; }
        public int ServicePort { get; set; }
        public string ServiceId => _serviceId;
        public string ServiceName => _serviceName;
        public bool IsRegistered { get { return _isRegistered; } internal set { _isRegistered = value; } }
        public TtlCheck TtlCheck { get { return _ttlCheck; } internal set { _ttlCheck = value; } }

        protected int GetNextAvailablePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Debug.Assert(_shutdownCounter.Wait(5000), "Did not shut down cleanly!!!");

            if (_disposed)
                return;

            if (disposing)
            {
            }
            _httpClient.Dispose();
            _disposed = true;
        }

        ~ServiceManager()
        {
            Dispose(false);
        }
    }
}
