using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Server.DataContracts;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class RoutingHost
    {
        private readonly string _healthCheckUri;
        private readonly string _serviceLookupUri;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly CustomRouter _router;
        private readonly HttpClient _client = new HttpClient();
        private readonly ILogger<RoutingHost> _logger;
        private readonly RoutingData _routingData;
        private readonly Func<IConsulService> _serviceFactory;
        private readonly Func<ICurrentState> _statsFactory;
        private string _lastConsulIndex = string.Empty;

        public RoutingHost(CustomRouter router, CondenserConfiguration config, ILoggerFactory logger,
            RoutingData routingData, IEnumerable<IService> customRoutes, Func<IConsulService> serviceFactory, 
            Func<ICurrentState> statsFactory)
        {
            _routingData = routingData;
            _serviceFactory = serviceFactory;
            _statsFactory = statsFactory;
            _logger = logger?.CreateLogger<RoutingHost>();
            _client.Timeout = TimeSpan.FromMinutes(6);
            _router = router;
            _healthCheckUri = $"http://{config.AgentAddress}:{config.AgentPort}{HttpUtils.HealthAnyUrl}?index=";
            _serviceLookupUri = $"http://{config.AgentAddress}:{config.AgentPort}{HttpUtils.SingleServiceCatalogUrl}";
            var ignore = WatchLoop();

            foreach (var customRoute in customRoutes)
            {
                _router.AddNewService(customRoute);
            }
        }

        public CustomRouter Router => _router;
        public Action<Dictionary<string, List<IService>>> OnRouteBuilt { get; set; }

        private async Task WatchLoop()
        {
            while (!_cancel.IsCancellationRequested)
            {
                try
                {
                    _logger?.LogInformation("Looking for health changes with index {index}", _lastConsulIndex);
                    var result = await _client.GetAsync(_healthCheckUri + _lastConsulIndex.ToString(), _cancel.Token);
                    if (!result.IsSuccessStatusCode)
                    {
                        _logger?.LogWarning("Retrieved a response that was not success when getting the health status code was {code}", result.StatusCode);
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                    _lastConsulIndex = result.GetConsulIndex();
                    _logger?.LogInformation("Got new set of health information new index is {index}", _lastConsulIndex);

                    var healthChecks = await result.Content.GetObject<HealthCheck[]>();
                    await ProcessHealthChecks(healthChecks);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(1000, ex, "There was an error getting available services from consul");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        private async Task ProcessHealthChecks(HealthCheck[] healthChecks)
        {
            _logger?.LogInformation("Total number of health checks returned was {healthCheckCount}", healthChecks.Length);
            List<InformationService> infoList = BuildListOfHealthyServiceInstances(healthChecks);
            RemoveDeadInstances(infoList);
            foreach (var service in _routingData.ServicesWithHealthChecks)
            {
                var infoService = await _client.GetAsync<ServiceInstance[]>(_serviceLookupUri + service.Key);
                foreach (var info in infoService)
                {
                    var instance = GetInstance(info, service.Value);
                    if (instance == null)
                    {
                        await CreateNewServiceInstance(service, info);
                    }
                    else
                    {
                        UpdateExistingRoutes(instance, info);
                    }
                }
            }
            _router.CleanUpRoutes();
            OnRouteBuilt?.Invoke(_routingData.ServicesWithHealthChecks);
        }

        private async Task CreateNewServiceInstance(KeyValuePair<string, List<IService>> service, ServiceInstance info)
        {
            var instance = _serviceFactory();

            if (!_routingData.Stats.TryGetValue(info.ServiceID, out ICurrentState stats))
            {
                stats = _statsFactory();
                _routingData.Stats.Add(info.ServiceID, stats);
            }

            await instance.Initialise(info.ServiceID, info.Node, info.ServiceTags, info.ServiceAddress, info.ServicePort, stats);
            _logger?.LogInformation("Adding a new service instance {serviceId} that is running the service {service} mapped to {routes}", instance.ServiceId, service.Key, instance.Routes);
            _router.AddNewService(instance);
            service.Value.Add(instance);
        }

        private void UpdateExistingRoutes(IService instance, ServiceInstance info)
        {
            var routes = ServiceUtils.RoutesFromTags(info.ServiceTags);
            if (instance.Routes.SequenceEqual(routes))
            {
                return;
            }
            foreach (var newTag in routes.Except(instance.Routes))
            {
                _router.AddServiceToRoute(newTag, instance);
            }
            foreach (var oldTag in instance.Routes.Except(routes))
            {
                _router.RemoveServiceFromRoute(oldTag, instance);
            }
            instance.UpdateRoutes(routes);
        }

        private IService GetInstance(ServiceInstance service, List<IService> instanceList)
        {
            for (int i = 0; i < instanceList.Count; i++)
            {
                if (instanceList[i].ServiceId == service.ServiceID)
                {
                    return instanceList[i];
                }
            }
            return null;
        }

        private void RemoveDeadInstances(List<InformationService> infoList)
        {
            //All services that are removed
            foreach (var service in _routingData.ServicesWithHealthChecks.ToArray())
            {
                foreach (var instance in service.Value.ToArray())
                {
                    if (!HasInstance(instance.ServiceId, infoList))
                    {
                        //Service is dead remove it
                        service.Value.Remove(instance);
                        _logger?.LogInformation("The service instance {serviceId} for service {serviceName} was removed due to failing health", instance.ServiceId, service.Key);
                        _router.RemoveService(instance);
                    }
                }
                if (service.Value.Count == 0)
                {
                    _routingData.ServicesWithHealthChecks.Remove(service.Key);
                }
            }
            foreach (var i in infoList)
            {
                if (!_routingData.ServicesWithHealthChecks.ContainsKey(i.Service))
                {
                    _logger?.LogInformation("New service {serviceName} added because we have found instances of it", i.Service);
                    _routingData.ServicesWithHealthChecks[i.Service] = new List<IService>();
                }
            }
        }

        private List<InformationService> BuildListOfHealthyServiceInstances(HealthCheck[] healthChecks)
        {
            HashSet<string> downNodes = new HashSet<string>();
            //Now we get all service instances that are working
            var infoList = new List<InformationService>();
            foreach (var check in healthChecks)
            {
                if ((check.CheckID == "serfHealth" || check.CheckID == "_node_maintenance") && check.Status != HealthCheckStatus.Passing)
                {
                    downNodes.Add(check.Node);
                }
            }
            _logger?.LogInformation("List of nodes that are currently down is {downNodes}", downNodes);
            foreach (var check in healthChecks)
            {
                if (!string.IsNullOrEmpty(check.ServiceID) && check.Status == HealthCheckStatus.Passing && !downNodes.Contains(check.Node))
                {
                    infoList.Add(new InformationService()
                    {
                        Service = check.ServiceName,
                        ID = check.ServiceID
                    });
                }
            }
            _logger?.LogInformation("Total number of instances with passing health checks is {passingHealthChecks}", infoList.Count);
            return infoList;
        }

        private bool HasInstance(string serviceID, List<InformationService> infoList)
        {
            for (int i = 0; i < infoList.Count; i++)
            {
                if (infoList[i].ID == serviceID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
