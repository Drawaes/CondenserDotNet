using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    class ServiceInformationContainer
    {
        ManualResetEvent _loadedHandle = new ManualResetEvent(false);
        static int seed = Environment.TickCount;
        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        HttpClient _client;
        string _requestString;
        JsonSerializerSettings _settings;
        CancellationToken _token;

        public ServiceInformationContainer(string serviceName, HttpClient client, JsonSerializerSettings settings)
        {
            ServiceName = serviceName;
            _client = client;
            _requestString = $"/v1/health/service/{serviceName}";
            _settings = settings;
            CheckForUpdates();
        }

        public string ServiceName { get; private set; }
        Dictionary<InformationService, List<Version>> _serviceListings = new Dictionary<InformationService, List<Version>>();
        
        public Tuple<string, int> GetServiceInstance(Version minVersion = null, Version maxVersion = null, Version exactVersion = null, int msTimeout = Timeout.Infinite)
        {
            if (!_loadedHandle.WaitOne(msTimeout))
                throw new TimeoutException("Waited for the service list to be retrieved");

            var listOfServices = _serviceListings.AsEnumerable();

            if (minVersion != null)
            {
                listOfServices = listOfServices.Where(s => s.Value.Any(v => v > minVersion));
            }
            if (maxVersion != null)
            {
                listOfServices = listOfServices.Where(s => s.Value.Any(v => v < maxVersion));
            }
            if (exactVersion != null)
            {
                listOfServices = listOfServices.Where(s => s.Value.Any(v => v == exactVersion));
            }

            var matchingServices = listOfServices.ToArray();

            if (matchingServices.Length == 0)
                return null;

            var service = matchingServices[random.Value.Next(0, matchingServices.Length)];

            return Tuple.Create(service.Key.Address, service.Key.Port);
        }

        private async Task CheckForUpdates(string index = null)
        {
            var result = await _client.GetAsync(_requestString + $"?index={index}", _token);
            IEnumerable<string> waitTime = null;
            result.Headers.TryGetValues("X-Consul-Index", out waitTime);
            var stringResult = await result.Content.ReadAsStringAsync();
            var objects = JsonConvert.DeserializeObject<InformationServiceSet[]>(stringResult);
            var dictionary = new Dictionary<InformationService, List<Version>>();
            foreach (var obj in objects)
            {
                var versions = new List<Version>();
                obj.Service.Checks = obj.Checks;
                foreach (var v in obj.Service.Tags)
                {
                    if (v.StartsWith("version="))
                    {
                        var vers = Version.Parse(v.Substring("version=".Length));
                        versions.Add(vers);
                    }
                }
                dictionary.Add(obj.Service, versions);
            }
            Volatile.Write(ref _serviceListings ,dictionary);
            _loadedHandle.Set();
            if (!_token.IsCancellationRequested)
            {
                CheckForUpdates(waitTime.FirstOrDefault());
            }
        }
    }
}
