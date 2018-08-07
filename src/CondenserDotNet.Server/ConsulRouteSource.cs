using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Consul;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class ConsulRouteSource : IRouteSource
    {
        private readonly HttpClient _client;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly string _healthCheckUri;
        private readonly string _serviceLookupUri;
        private string _lastConsulIndex = string.Empty;
        private readonly ILogger _logger;
        static readonly HealthCheck[] EmptyChecks = new HealthCheck[0];

        public ConsulRouteSource(CondenserConfiguration config,
            ILoggerFactory logger, IConsulAclProvider aclProvider = null)
        {
            _client = HttpUtils.CreateClient(aclProvider, config.AgentAddress, config.AgentPort);
            _healthCheckUri = $"{HttpUtils.HealthAnyUrl}?index=";
            _serviceLookupUri = $"{HttpUtils.SingleServiceCatalogUrl}";

            _logger = logger?.CreateLogger<ConsulRouteSource>();
        }

        public bool CanRequestRoute() => !_cancel.IsCancellationRequested;

        public async Task<GetHealthCheckResult> TryGetHealthChecksAsync()
        {
            _logger?.LogInformation("Looking for health changes with index {index}", _lastConsulIndex);
            var result = await _client.GetAsync(_healthCheckUri + _lastConsulIndex.ToString(), _cancel.Token);
            if (!result.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Retrieved a response that was not success when getting the health status code was {code}", result.StatusCode);
                return new GetHealthCheckResult() { Checks = EmptyChecks, Success = false };
            }
            var newConsulIndex = result.GetConsulIndex();

            if (newConsulIndex  == _lastConsulIndex)
            {
                return new GetHealthCheckResult() { Success = false, Checks = EmptyChecks };
            }

            _lastConsulIndex = newConsulIndex ;

            _logger?.LogInformation("Got new set of health information new index is {index}", _lastConsulIndex);

            var checks = await result.Content.GetObject<HealthCheck[]>();
            return new GetHealthCheckResult() { Success = true, Checks = checks };
        }

        public Task<ServiceInstance[]> GetServiceInstancesAsync(string serviceName) => _client.GetAsync<ServiceInstance[]>(_serviceLookupUri + serviceName);
    }
}
