using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    public class ServiceManager : IDisposable
    {
        private HttpClient _httpClient;
        private JsonSerializerSettings _jsonSettings;
        private bool _disposed = false;

        public ServiceManager() : this("localhost", 8500) { }
        public ServiceManager(int agentPort) : this("localhost", agentPort) { }
        public ServiceManager(string agentAddress) : this(agentAddress, 8500) { }
        public ServiceManager(string agentAddress, int agentPort)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"http://{agentAddress}:{agentPort}");
            _jsonSettings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver(), NullValueHandling = NullValueHandling.Ignore };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
