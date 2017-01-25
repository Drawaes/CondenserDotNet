using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;

namespace CondenserDotNet.Core
{
    internal class ServiceWatcher
    {
        //used to randomly select an instance
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> Random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        private readonly BlockingWatcher<InformationServiceSet[]> _watcher;

        public ServiceWatcher(string serviceName,
            HttpClient client, CancellationToken cancel)
        {
            string lookupUrl = $"{HttpUtils.ServiceHealthUrl}{serviceName}";
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _watcher = new BlockingWatcher<InformationServiceSet[]>(lookupUrl, client, cancel);
            _watcher.WatchLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        internal async Task<InformationService> GetNextServiceInstanceAsync()
        {
            var instances = await _watcher.ReadAsync();
            if ((instances != null) && (instances.Length > 0))
                return instances[Random.Value.Next(0, instances.Length)].Service;
            return null;
        }
    }
}