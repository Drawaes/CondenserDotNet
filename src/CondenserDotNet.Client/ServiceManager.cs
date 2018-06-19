using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CondenserDotNet.Client
{
    public class ServiceManager : IServiceManager
    {
        private readonly List<string> _supportedUrls = new List<string>();
        private readonly List<string> _customTags = new List<string>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private bool _disposed;
        private ITtlCheck _ttlCheck;
        private Task _registrationTask;

        public ServiceManager(IOptions<ServiceManagerConfig> optionsConfig, Func<HttpClient> httpClientFactory = null, ILoggerFactory logFactory = null, IServer server = null, IConsulAclProvider aclProvider = null)
        {
            if (optionsConfig.Value.ServicePort == 0 && server == null)
            {
                throw new ArgumentOutOfRangeException($"A valid server port needs to be set through either the options or the hosting server");
            }

            var config = optionsConfig.Value;
            Logger = logFactory?.CreateLogger<ServiceManager>();
            Client = httpClientFactory?.Invoke() ?? HttpUtils.CreateClient(aclProvider);
            config.SetDefaults(server);
            ServiceId = config.ServiceId;
            ServiceName = config.ServiceName;
            ServiceAddress = config.ServiceAddress;
            ServicePort = config.ServicePort;
        }

        public List<string> SupportedUrls => _supportedUrls;
        public List<string> CustomTags => _customTags;
        public ILogger Logger { get; }
        public HttpClient Client { get; }
        public HealthConfiguration HealthConfig { get; private set; } = new HealthConfiguration();
        public Service RegisteredService { get; set; }
        public string ServiceId { get; }
        public string ServiceName { get; }
        public TimeSpan DeregisterIfCriticalAfter { get; set; }
        public bool IsRegistered => RegisteredService != null;
        public ITtlCheck TtlCheck { get => _ttlCheck; set => _ttlCheck = value; }
        public string ServiceAddress { get; }
        public int ServicePort { get; }
        public CancellationToken Cancelled => _cancel.Token;
        public string ProtocolSchemeTag { get; set; }
        public Task RegistrationTask => Volatile.Read(ref _registrationTask);

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
                Client.Dispose();
                _disposed = true;
            }
        }

        public bool UpdateRegistrationTask(Task inboundTask)
        {
            var originalValue = Interlocked.Exchange(ref _registrationTask, inboundTask);
            return originalValue == null ? true : false;
        }

        ~ServiceManager() => Dispose(false);
    }
}
