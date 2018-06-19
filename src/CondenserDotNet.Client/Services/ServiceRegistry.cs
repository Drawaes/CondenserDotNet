using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using CondenserDotNet.Core.Consul;

namespace CondenserDotNet.Client.Services
{
    public class ServiceRegistry : IServiceRegistry, IDisposable
    {
        private readonly HttpClient _client;
        private readonly Dictionary<string, ServiceWatcher> _watchedServices = new Dictionary<string, ServiceWatcher>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger _logger;

        public ServiceRegistry(Func<HttpClient> httpClientFactory = null, ILoggerFactory loggerFactory = null, IConsulAclProvider aclProvider = null)
        {
            _logger = loggerFactory?.CreateLogger<ServiceRegistry>();
            _client = httpClientFactory?.Invoke() ?? HttpUtils.CreateClient(aclProvider);
        }

        public async Task<IEnumerable<string>> GetAvailableServicesAsync()
        {
            var all = await GetAvailableServicesWithTagsAsync();
            return all.Keys;
        }

        public async Task<Dictionary<string, string[]>> GetAvailableServicesWithTagsAsync()
        {
            var result = await _client.GetAsync(HttpUtils.ServiceCatalogUrl);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await result.Content.ReadAsStringAsync();
            var serviceList = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(content);
            return serviceList;
        }

        public Task<InformationService> GetServiceInstanceAsync(string serviceName)
        {
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out var watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client
                        , RandomRoutingStrategy<InformationServiceSet>.Default, _logger, useNearest: false);
                    _watchedServices.Add(serviceName, watcher);
                }
                else
                {
                    if(watcher.IsNearest)
                    {
                        watcher.IsNearest = false;
                        watcher.RoutingStrategy = RandomRoutingStrategy<InformationServiceSet>.Default;
                        _logger?.LogInformation("Changed service lookup type for service of {serviceName} from nearest to any random", serviceName);
                    }
                }
                //We either have one or have made one now so lets carry on
                return watcher.GetNextServiceInstanceAsync();
            }
        }

        public Task<InformationService> GetNearestServiceInstanceAsync(string serviceName)
        {
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out var watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client, UseTopRouting<InformationServiceSet>.Default, _logger, useNearest: true);
                    _watchedServices.Add(serviceName, watcher);
                }
                else
                {
                    if(!watcher.IsNearest)
                    {
                        watcher.IsNearest = true;
                        watcher.RoutingStrategy = UseTopRouting<InformationServiceSet>.Default;
                        _logger.LogInformation("Changed service lookup type for service of {serviceName} from any random to nearest", serviceName);
                    }
                }
                return watcher.GetNextServiceInstanceAsync();
            }
        }

        public void SetServiceListCallback(string serviceName, Action<List<InformationServiceSet>> callback)
        {
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out var watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client
                        , new RandomRoutingStrategy<InformationServiceSet>(), _logger, useNearest: false);
                    _watchedServices.Add(serviceName, watcher);
                }
                watcher.SetCallback(callback);
            }
        }

        public WatcherState GetServiceCurrentState(string serviceName)
        {
            lock (_watchedServices)
            {
                if (!_watchedServices.TryGetValue(serviceName, out var watcher))
                {
                    watcher = new ServiceWatcher(serviceName, _client
                        , new RandomRoutingStrategy<InformationServiceSet>(), _logger, useNearest: false);
                    _watchedServices.Add(serviceName, watcher);
                }
                return watcher.State;
            }
        }

        public void Dispose()
        {
            lock (_watchedServices)
            {
                foreach (var kv in _watchedServices)
                {
                    kv.Value.Dispose();
                }
            }
        }

        public ServiceBasedHttpHandler GetHttpHandler() => new ServiceBasedHttpHandler(this, 20);
        public ServiceBasedHttpHandler GetHttpHandler(int maxConnectionsPerServer) => new ServiceBasedHttpHandler(this, maxConnectionsPerServer);
    }
}
