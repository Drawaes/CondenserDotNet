using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Client.Services
{
    public class ServiceRegistry : IServiceRegistry, IDisposable
    {
        private readonly HttpClient _client;
        private readonly Dictionary<string, ServiceWatcher> _watchedServices = new Dictionary<string, ServiceWatcher>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger _logger;

        public ServiceRegistry(Func<HttpClient> httpClientFactory = null, ILoggerFactory loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger<ServiceRegistry>();
            _client = httpClientFactory?.Invoke() ?? new HttpClient() { BaseAddress = new Uri("http://localhost:8500") };
        }

        public async Task<IEnumerable<string>> GetAvailableServicesAsync()
        {
            var all = await GetAvailableServicesWithTagsAsync();
            return all.Keys;
        }

        public async Task<Dictionary<string, string[]>> GetAvailableServicesWithTagsAsync()
        {
            var result = await _client.GetAsync(HttpUtils.ServiceCatalogUrl);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await result.Content.ReadAsStringAsync();
            var serviceList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
            return serviceList;
        }

        public Task<InformationService> GetServiceInstanceAsync(string serviceName)
        {
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out ServiceWatcher watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client
                        , new RandomRoutingStrategy<InformationServiceSet>(), _logger);
                    _watchedServices.Add(serviceName, watcher);
                }
                //We either have one or have made one now so lets carry on
                return watcher.GetNextServiceInstanceAsync();
            }
        }

        public WatcherState GetServiceCurrentState(string serviceName)
        {
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out ServiceWatcher watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client
                        , new RandomRoutingStrategy<InformationServiceSet>(), _logger);
                    _watchedServices.Add(serviceName, watcher);
                }
                return watcher.State;
            }
        }

        public void Dispose()
        {
            lock (_watchedServices)
            {
                foreach (var kv in _watchedServices)
                {
                    kv.Value.Dispose();
                }
            }
        }

        public ServiceBasedHttpHandler GetHttpHandler() => new ServiceBasedHttpHandler(this);
    }
}
