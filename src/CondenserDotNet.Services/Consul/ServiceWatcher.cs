using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Services.Consul
{
    public class ServiceWatcher
    {
        private readonly BlockingWatcher<List<ServiceInformationSet>> _watcher;
        private const string HealthUrl = "/v1/health/service/";

        private ThreadLocal<Random> _randoms = new ThreadLocal<Random>(() =>
        {
            var value = Interlocked.Increment(ref _currentSeed);
            return new Random(value);
        });
        private static int _currentSeed = DateTime.UtcNow.Millisecond;

        public ServiceWatcher(string serviceName, HttpClient client, CancellationToken cancel)
        {
            string lookupUrl = $"{HealthUrl}{serviceName}";
            _watcher = new BlockingWatcher<List<ServiceInformationSet>>(lookupUrl, client, cancel);
            var ignore = _watcher.WatchLoop();
        }

        internal async Task<ServiceInformation> GetNextServiceInstanceAsync()
        {
            var instances = await _watcher.ReadAsync();
            return instances[_randoms.Value.Next(instances.Count-1)].Service;
        }
    }
}
