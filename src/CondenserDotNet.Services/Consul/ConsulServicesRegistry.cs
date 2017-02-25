using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CondenserDotNet.Services.Consul
{
    public class ConsulServicesRegistry : IServicesRegistry
    {
        private readonly HttpClient _client;
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();
        private readonly Dictionary<string, ServiceWatcher> _watchedServices = new Dictionary<string, ServiceWatcher>(StringComparer.OrdinalIgnoreCase);
        private const string ServiceCatalogUrl = "/v1/catalog/services";
        private ConsulServicesConfig _config;

        public ConsulServicesRegistry(IOptions<ConsulServicesConfig> config)
        {
            _config = config?.Value ?? new ConsulServicesConfig();
            _client = _config.HttpClientFactory();
        }

        public async Task<Dictionary<string, string[]>> GetAvailableServices()
        {
            var result = await _client.GetAsync(ServiceCatalogUrl, _cancelToken.Token);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await result.Content.ReadAsStringAsync();
            var serviceList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
            return serviceList;
        }

        public Task<ServiceInformation> GetServiceInstanceAsync(string serviceName)
        {
            ServiceWatcher watcher;
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client, _cancelToken.Token);
                    _watchedServices.Add(serviceName, watcher);
                }
            }
            //We either have one or have made one now so lets carry on
            return watcher.GetNextServiceInstanceAsync();
        }

        public Task<Dictionary<string, string[]>> GetAvailableServicesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<T> GetFromService<T>(string serviceName, string url)
        {
            throw new NotImplementedException();
        }

        public Task<TOutput> PostToService<TInput, TOutput>(string serviceName, TInput objectToPost, string url)
        {
            throw new NotImplementedException();
        }
    }
}
