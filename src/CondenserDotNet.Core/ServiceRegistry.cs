using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CondenserDotNet.Core
{
    public class ServiceRegistry : IServiceRegistry
    {
        private readonly HttpClient _client;
        private readonly CancellationToken _cancel;
        private readonly Dictionary<string, ServiceWatcher> _watchedServices = new Dictionary<string, ServiceWatcher>(StringComparer.OrdinalIgnoreCase);

        public ServiceRegistry(
            HttpClient client, CancellationToken cancel)
        {
            _client = client;
            _cancel = cancel;
        }

        public async Task<IEnumerable<string>> GetAvailableServicesAsync()
        {
            var all = await GetAvailableServicesWithTagsAsync();
            return all.Keys;
        }

        public async Task<Dictionary<string, string[]>> GetAvailableServicesWithTagsAsync()
        {
            var result = await _client.GetAsync(
                HttpUtils.ServiceCatalogUrl, _cancel);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await result.Content.ReadAsStringAsync();
            var serviceList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
            return serviceList;
        }

        public Task<DataContracts.InformationService> GetServiceInstanceAsync(string serviceName)
        {
            ServiceWatcher watcher;
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client, _cancel);
                    _watchedServices.Add(serviceName, watcher);
                }
            }
            //We either have one or have made one now so lets carry on
            return watcher.GetNextServiceInstanceAsync();
        }
    }
}
