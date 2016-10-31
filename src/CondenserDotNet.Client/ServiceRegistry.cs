using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.Internal;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class ServiceRegistry
    {
        private readonly ServiceManager _serviceManager;
        private readonly Dictionary<string, ServiceWatcher> _watchedServices = new Dictionary<string, ServiceWatcher>(StringComparer.OrdinalIgnoreCase);

        internal ServiceRegistry(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<IEnumerable<string>> GetAvailableServicesAsync()
        {
            var result = await _serviceManager.Client.GetAsync(HttpUtils.ServiceCatalogUrl, _serviceManager.Cancelled);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await result.Content.ReadAsStringAsync();
            var serviceList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
            return serviceList.Keys;
        }

        public Task<DataContracts.InformationService> GetServiceInstanceAsync(string serviceName)
        {
            ServiceWatcher watcher;
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out watcher))
                {
                    watcher = new ServiceWatcher(_serviceManager, serviceName);
                    _watchedServices.Add(serviceName, watcher);
                }
            }
            //We either have one or have made one now so lets carry on
            return watcher.GetNextServiceInstanceAsync();
        }
    }
}
