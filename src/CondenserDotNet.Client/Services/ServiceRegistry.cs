using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace CondenserDotNet.Client.Services
{
    public class ServiceRegistry : IServiceRegistry, IDisposable
    {
        private readonly HttpClient _client;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
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
            var result = await _client.GetAsync(HttpUtils.ServiceCatalogUrl, _cancel.Token);
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
            ServiceWatcher watcher;
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client
                        , new RandomRoutingStrategy<InformationServiceSet>(), _logger);
                    _watchedServices.Add(serviceName, watcher);
                }
            }
            //We either have one or have made one now so lets carry on
            return watcher.GetNextServiceInstanceAsync();
        }

        public void Dispose()
        {
            _cancel.Cancel();
            _client.Dispose();
        }

        public ServiceBasedHttpHandler GetHttpHandler()
        {
            return new ServiceBasedHttpHandler(this);
        }
    }
}
