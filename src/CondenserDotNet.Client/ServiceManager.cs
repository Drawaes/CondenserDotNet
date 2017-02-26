using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CondenserDotNet.Client.DataContracts;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Client
{
    public class ServiceManager : IServiceManager
    {
        private readonly HttpClient _httpClient;
        private bool _disposed;
        private readonly string _serviceName;
        private readonly string _serviceId;
        private readonly List<string> _supportedUrls = new List<string>();
        private ITtlCheck _ttlCheck;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly ILogger _logger;

        public ServiceManager(IOptions<ServiceManagerConfig> optionsConfig, Func<HttpClient> httpClientFactory = null, ILoggerFactory logFactory = null, IServer server = null)
        {
            if (optionsConfig.Value.ServicePort == 0 && server == null)
            {
                throw new ArgumentOutOfRangeException($"A valid server port needs to be set through either the options or the hosting server");
            }

            var config = optionsConfig.Value;
            _logger = logFactory?.CreateLogger<ServiceManager>();
            _httpClient = httpClientFactory?.Invoke() ?? new HttpClient() { BaseAddress = new Uri("http://localhost:8500") };
            _serviceId = config.ServiceId;
            _serviceName = config.ServiceName;
            ServiceAddress = config.ServiceAddress;
            if (config.ServicePort > 0)
            {
                ServicePort = config.ServicePort;
            }
            else
            {
                var feature = server.Features.Get<IServerAddressesFeature>();
                ServicePort = new Uri(feature.Addresses.First()).Port;
            }
        }

        public List<string> SupportedUrls => _supportedUrls;
        public HttpClient Client => _httpClient;
        public HealthCheck HttpCheck { get; set; }
        public Service RegisteredService { get; set; }
        public string ServiceId => _serviceId;
        public string ServiceName => _serviceName;
        public TimeSpan DeregisterIfCriticalAfter { get; set; }
        public bool IsRegistered => RegisteredService != null;
        public ITtlCheck TtlCheck { get => TtlCheck1; set => TtlCheck1 = value; }
        public string ServiceAddress { get; }
        public int ServicePort { get; }
        public CancellationToken Cancelled => _cancel.Token;
        public ITtlCheck TtlCheck1 { get => _ttlCheck; set => _ttlCheck = value; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed) return;

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
