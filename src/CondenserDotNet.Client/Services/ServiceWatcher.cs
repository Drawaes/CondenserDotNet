using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Client.Services
{
    internal class ServiceWatcher
    {
        private readonly BlockingWatcher<List<InformationServiceSet>> _watcher;
        private readonly IRoutingStrategy<InformationServiceSet> _routingStrategy;
        private readonly Task _watcherTask;
        private readonly ILogger _logger;
        private readonly string _serviceName;

        internal ServiceWatcher(string serviceName, HttpClient client, CancellationToken cancel,
            IRoutingStrategy<InformationServiceSet> routingStrategy, ILogger logger)
        {
            _serviceName = serviceName;
            _logger = logger;
            _routingStrategy = routingStrategy;
            string lookupUrl = $"{HttpUtils.ServiceHealthUrl}{serviceName}?passing&index=";
            Func<string,Task<HttpResponseMessage>> action = 
                (consulIndex) => client.GetAsync(lookupUrl + consulIndex, cancel);
            _watcher = new BlockingWatcher<List<InformationServiceSet>>(action);
            _watcherTask = _watcher.WatchLoop();
        }

        internal async Task<InformationService> GetNextServiceInstanceAsync()
        {
            try
            {
                var instances = await _watcher.ReadAsync();
                return _routingStrategy.RouteTo(instances)?.Service;
            }
            catch(Exception ex)
            {
                _logger?.LogError("Unable to get an instance of {serviceName} the error was {excception}",_serviceName, ex);
                throw new NoServiceInstanceFoundException(_serviceName,ex);
            }
        }
    }
}