using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Services.Consul
{
    public class ServiceWatcher
    {
        //used to randomly select an instance


        private readonly BlockingWatcher<List<InformationServiceSet>> _watcher;
        private readonly IRoutingStrategy<InformationServiceSet> _routingStrategy;

        public ServiceWatcher(string serviceName, HttpClient client, CancellationToken cancel)
        {
            _routingStrategy = routingStrategy;
            string lookupUrl = $"{HttpUtils.ServiceHealthUrl}{serviceName}";
            _watcher = new BlockingWatcher<List<InformationServiceSet>>(lookupUrl, client, cancel);
            _watcher.WatchLoop();
        }

        internal async Task<InformationService> GetNextServiceInstanceAsync()
        {
            var instances = await _watcher.ReadAsync();
            return _routingStrategy.RouteTo(instances)?.Service;
        }
    }
}
