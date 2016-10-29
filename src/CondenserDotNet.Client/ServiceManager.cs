using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private string _serviceAddress;
        private int _servicePort;
        private bool _isRegistered;
        private List<string> _supportedUrls = new List<string>();
        private HealthCheck _httpCheck;
        private TtlCheck _ttlCheck;

        public ServiceManager(string serviceName) : this(serviceName, $"{serviceName}:{Dns.GetHostName()}", "localhost", 8500) { }
        public ServiceManager(string serviceName, string serviceId) : this(serviceName, serviceId, "localhost", 8500) { }
        public ServiceManager(string serviceName, string agentAddress, int agentPort) : this(serviceName, $"{serviceName}:{Dns.GetHostName()}", agentAddress, agentPort) { }
        public ServiceManager(string serviceName, string serviceId, string agentAddress, int agentPort)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"http://{agentAddress}:{agentPort}");
            _serviceId = serviceId;
            _serviceName = serviceName;
        }

        internal List<string> SupportedUrls => _supportedUrls;
        internal HttpClient Client => _httpClient;
        internal HealthCheck HttpCheck { get { return _httpCheck; } set { _httpCheck = value; } }
        public string ServiceAddress => _serviceAddress;
        public int ServicePort => _servicePort;
        public string ServiceId => _serviceId;
        public string ServiceName => _serviceName;
        public bool IsRegistered { get { return _isRegistered; } internal set { _isRegistered = value; } }
        public TtlCheck TtlCheck { get { return _ttlCheck; } internal set { _ttlCheck = value; } }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
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
