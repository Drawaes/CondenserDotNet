using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;

namespace CondenserDotNet.Client.Services
{
    internal class ServiceWatcher
    {
        private readonly BlockingWatcher<List<InformationServiceSet>> _watcher;
        private readonly IRoutingStrategy<InformationServiceSet> _routingStrategy;
        private readonly Task _watcherTask;

        internal ServiceWatcher(string serviceName, HttpClient client, CancellationToken cancel,
            IRoutingStrategy<InformationServiceSet> routingStrategy)
        {
            _routingStrategy = routingStrategy;
            string lookupUrl = $"{HttpUtils.ServiceHealthUrl}{serviceName}?passing&index=";
            Func<string,Task<HttpResponseMessage>> action = 
                (consulIndex) => client.GetAsync(lookupUrl + consulIndex, cancel);
            _watcher = new BlockingWatcher<List<InformationServiceSet>>(action);
            _watcherTask = _watcher.WatchLoop();
        }

        internal async Task<InformationService> GetNextServiceInstanceAsync()
        {
            var instances = await _watcher.ReadAsync();
            return _routingStrategy.RouteTo(instances)?.Service;
        }
    }
}