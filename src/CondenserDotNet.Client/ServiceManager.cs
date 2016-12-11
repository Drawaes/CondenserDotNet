﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    public class ServiceManager : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed;
        private readonly string _serviceName;
        private readonly string _serviceId;
        private readonly List<string> _supportedUrls = new List<string>();
        private HealthCheck _httpCheck;
        private TtlCheck _ttlCheck;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly ConfigurationRegistry _config;
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
            _config = new ConfigurationRegistry(this);
            _services = new ServiceRegistry(this);
            _leaders = new LeaderRegistry(this);
            ServiceAddress = Dns.GetHostName();
            ServicePort = GetNextAvailablePort();
        }

        internal List<string> SupportedUrls => _supportedUrls;
        internal HttpClient Client => _httpClient;
        internal HealthCheck HttpCheck { get { return _httpCheck; } set { _httpCheck = value; } }
        internal Service RegisteredService { get; set; }
        public ConfigurationRegistry Config => _config;
        public string ServiceId => _serviceId;
        public string ServiceName => _serviceName;
        public TimeSpan DeregisterIfCriticalAfter { get; internal set; }
        public ServiceRegistry Services => _services;
        public bool IsRegistered => RegisteredService != null;
        public TtlCheck TtlCheck { get { return _ttlCheck; } internal set { _ttlCheck = value; } }
        public string ServiceAddress { get; set; }
        public int ServicePort { get; set; }
        public CancellationToken Cancelled => _cancel.Token;
        public LeaderRegistry Leaders => _leaders;

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
