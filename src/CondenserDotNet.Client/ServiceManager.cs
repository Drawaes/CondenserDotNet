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
        
        private bool _disposed;
        private readonly string _serviceName;
        private readonly string _serviceId;
        private readonly List<string> _supportedUrls = new List<string>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly LeaderRegistry _leaders;
        
        public ServiceManager(string serviceName, string agentAddress, int agentPort) : this(serviceName, $"{serviceName}:{Dns.GetHostName()}", agentAddress, agentPort) { }
        public ServiceManager(string serviceName, string serviceId, string agentAddress, int agentPort)
        {
            _serviceId = serviceId;
            _serviceName = serviceName;
            _leaders = new LeaderRegistry(this);
           
        }

        public List<string> SupportedUrls => _supportedUrls;
        public Service RegisteredService { get; set; }
        public string ServiceId => _serviceId;
        public string ServiceName => _serviceName;
        public TimeSpan DeregisterIfCriticalAfter { get; set; }
        public bool IsRegistered => RegisteredService != null;
        public string ServiceAddress { get; set; }
        public int ServicePort { get; set; }
        public CancellationToken Cancelled => _cancel.Token;
        public ILeaderRegistry Leaders => _leaders;

        

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
                //_httpClient.Dispose();
                _disposed = true;
            }
        }

        ~ServiceManager()
        {
            Dispose(false);
        }
    }
}
