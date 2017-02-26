using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Client.Services;
using CondenserDotNet.Core;
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
        private readonly IServiceRegistry _services;
        private readonly LeaderRegistry _leaders;
        private const int ConsulPort = 8500;
        private const string LocalHost = "localhost";

        public ServiceManager(IOptions<ServiceManagerConfig> optionsConfig, IServiceRegistry serviceRegistry)
        {
            var config = optionsConfig.Value;
            _httpClient = new HttpClient { BaseAddress = new Uri($"http://{config.AgentAddress}:{config.AgentPort}") };
            _serviceId = config.ServiceId;
            _serviceName = config.ServiceName;
            _services = serviceRegistry;
            _leaders = new LeaderRegistry(this);
            ServiceAddress = config.ServiceAddress;
            ServicePort = config.ServicePort;
        }

        public List<string> SupportedUrls => _supportedUrls;
        public HttpClient Client => _httpClient;
        public HealthCheck HttpCheck { get; set; }
        public Service RegisteredService { get; set; }
        public string ServiceId => _serviceId;
        public string ServiceName => _serviceName;
        public TimeSpan DeregisterIfCriticalAfter { get; set; }
        public IServiceRegistry Services => _services;
        public bool IsRegistered => RegisteredService != null;
        public ITtlCheck TtlCheck { get { return TtlCheck1; } set { TtlCheck1 = value; } }
        public string ServiceAddress { get; set; }
        public int ServicePort { get; set; }
        public CancellationToken Cancelled => _cancel.Token;
        public ILeaderRegistry Leaders => _leaders;

        public ITtlCheck TtlCheck1 { get => _ttlCheck; set => _ttlCheck = value; }

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
