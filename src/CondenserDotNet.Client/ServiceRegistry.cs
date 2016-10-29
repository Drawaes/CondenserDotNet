using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client.Internal;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class ServiceRegistry
    {
        private readonly ServiceManager _serviceManager;

        internal ServiceRegistry(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<IEnumerable<string>> GetAvailableServicesAsync()
        {
            var result = await _serviceManager.Client.GetAsync(HttpUtils.ServiceCatalogUrl);
            if(!result.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await result.Content.ReadAsStringAsync();
            var serviceList = JsonConvert.DeserializeObject<Dictionary<string,string[]>>(content);
            return serviceList.Keys;
        }
    }
}
