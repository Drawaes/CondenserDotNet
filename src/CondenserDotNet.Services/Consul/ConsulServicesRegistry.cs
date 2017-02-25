using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CondenserDotNet.Services.Consul
{
    public class ConsulServicesRegistry:IServicesRegistry
    {
        private readonly HttpClient _client;
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();
        private readonly Dictionary<string, ServiceWatcher> _watchedServices = new Dictionary<string, ServiceWatcher>(StringComparer.OrdinalIgnoreCase);

        public async Task<Dictionary<string, string[]>> GetAvailableServices()
        {
            var result = await _client.GetAsync(HttpUtils.ServiceCatalogUrl, _cancelToken.Token);
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
                    watcher = new ServiceWatcher(serviceName, _client, _cancel,
                        new RandomRoutingStrategy<InformationServiceSet>());
                    _watchedServices.Add(serviceName, watcher);
                }
            }
            //We either have one or have made one now so lets carry on
            return watcher.GetNextServiceInstanceAsync();
        }
    }
}
