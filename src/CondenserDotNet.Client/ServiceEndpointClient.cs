using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    public class ServiceEndpointClient: ClientBase
    {
        public string DataCenter { get; set;}
                
        public string[] AvailableDataCenters { get;set;}

        Dictionary<string, string[]> _availableServices = new Dictionary<string, string[]>();
        Dictionary<string, ServiceInformationContainer> _serviceSubscriptions = new Dictionary<string, ServiceInformationContainer>();

        public async Task<string[]> LoadAvailableDataCenters()
        {
            var returnString = await _httpClient.GetStringAsync("/v1/catalog/datacenters");
            var returnValues = JsonConvert.DeserializeObject<string[]>(returnString);
            AvailableDataCenters = returnValues;
            return returnValues;
        }

        public async Task LoadAvailableServices()
        {
            var returnString = await _httpClient.GetStringAsync("/v1/catalog/services");
            var services = JsonConvert.DeserializeObject<Dictionary<string,string[]>>(returnString);
            _availableServices = services;
        }

        private ServiceInformationContainer RegisterInterest(string serviceName)
        {
            _serviceSubscriptions.Add(serviceName, new ServiceInformationContainer(serviceName, _httpClient, _jsonSettings));
            return _serviceSubscriptions[serviceName];
        }

        public Tuple<string, int> GetServiceAddress(string serviceName, Version minVersion = null, Version maxVersion = null, Version exactVersion = null)
        {
            ServiceInformationContainer info;
            if(!_serviceSubscriptions.TryGetValue(serviceName, out info))
            {
                info = RegisterInterest(serviceName);
            }
            return info.GetServiceInstance(minVersion, maxVersion, exactVersion);
        }
        
    }
}
