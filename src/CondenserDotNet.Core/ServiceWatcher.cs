using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;

namespace CondenserDotNet.Core
{
    internal class ServiceWatcher
    {
        //used to randomly select an instance
        

        private readonly BlockingWatcher<List<InformationServiceSet>> _watcher;
        private readonly IRoutingStrategy<InformationServiceSet> _routingStrategy;

        public ServiceWatcher(string serviceName,
            HttpClient client, CancellationToken cancel,
            IRoutingStrategy<InformationServiceSet> routingStrategy)
        {
            _routingStrategy = routingStrategy;
            string lookupUrl = $"{HttpUtils.ServiceHealthUrl}{serviceName}";
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _watcher = new BlockingWatcher<List<InformationServiceSet>>(lookupUrl, client, cancel);
            _watcher.WatchLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        internal async Task<InformationService> GetNextServiceInstanceAsync()
        {
            var instances = await _watcher.ReadAsync();
            return _routingStrategy.RouteTo(instances)?.Service;
        }
    }
}