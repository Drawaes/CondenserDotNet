using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class ConsulRouteSource : IRouteSource
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly string _healthCheckUri;
        private readonly string _serviceLookupUri;
        private string _lastConsulIndex = string.Empty;
        private readonly ILogger _logger;
        readonly static HealthCheck[] EmptyChecks = new HealthCheck[0];

        public ConsulRouteSource(CondenserConfiguration config,
            ILoggerFactory logger)
        {
            _client.Timeout = TimeSpan.FromMinutes(6);

            _healthCheckUri = $"http://{config.AgentAddress}:{config.AgentPort}{HttpUtils.HealthAnyUrl}?index=";
            _serviceLookupUri = $"http://{config.AgentAddress}:{config.AgentPort}{HttpUtils.SingleServiceCatalogUrl}";

            _logger = logger?.CreateLogger<ConsulRouteSource>();
        }

        public bool CanRequestRoute()
        {
            return !_cancel.IsCancellationRequested;
        }

        public async Task<(bool success, HealthCheck[] checks)> TryGetHealthChecksAsync()
        {
            _logger?.LogInformation("Looking for health changes with index {index}", _lastConsulIndex);
            var result = await _client.GetAsync(_healthCheckUri + _lastConsulIndex.ToString(), _cancel.Token);
            if (!result.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Retrieved a response that was not success when getting the health status code was {code}", result.StatusCode);
                return (false, EmptyChecks);
            }
            _lastConsulIndex = result.GetConsulIndex();
            _logger?.LogInformation("Got new set of health information new index is {index}", _lastConsulIndex);

            var checks = await result.Content.GetObject<HealthCheck[]>();
            return (true, checks);
        }

        public Task<ServiceInstance[]> GetServiceInstancesAsync(string serviceName)
        {
            return _client.GetAsync<ServiceInstance[]>(_serviceLookupUri + serviceName);
        }
    }
}
