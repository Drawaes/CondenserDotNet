using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class ServiceEndpointClient : ClientBase
    {
        private const string _serviceCatalog = "/v1/catalog/services";
        private const string _datacenterCatalog = "/v1/catalog/datacenters";

        Dictionary<string, string[]> _availableServices = new Dictionary<string, string[]>();
        ConcurrentDictionary<string, ServiceInformationContainer> _serviceSubscriptions = new ConcurrentDictionary<string, ServiceInformationContainer>();
                

        public string DataCenter { get; set; }

        public string[] AvailableDataCenters { get; set; }
        
        public async Task<string[]> LoadAvailableDataCenters()
        {
            var returnString = await _httpClient.GetStringAsync(_datacenterCatalog);
            var returnValues = JsonConvert.DeserializeObject<string[]>(returnString);
            AvailableDataCenters = returnValues;
            return returnValues;
        }

        public async Task LoadAvailableServices()
        {
            var returnString = await _httpClient.GetStringAsync(_serviceCatalog);
            var services = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(returnString);
            _availableServices = services;
        }

        //public Task<Tuple<string, int>> GetServiceAddressAsync(string serviceName, Version minVersion = null, Version maxVersion = null, Version exactVersion = null)
        //{
        //    ServiceInformationContainer info = _serviceSubscriptions.GetOrAdd(serviceName, (key) => new ServiceInformationContainer(serviceName, _httpClient, _jsonSettings));

        //    return info.GetServiceInstance(minVersion, maxVersion, exactVersion);
        //}

    }
}
