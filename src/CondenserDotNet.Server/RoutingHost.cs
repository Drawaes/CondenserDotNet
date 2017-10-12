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
using CondenserDotNet.Core.Routing;

namespace CondenserDotNet.Server
{
    public class RoutingHost
    {
        private readonly CustomRouter _router;
        private readonly ILogger<RoutingHost> _logger;
        private readonly IRouteStore _store;
        private readonly IRouteSource _source;
        private readonly IRoutingConfig _config;

        public RoutingHost(ILoggerFactory logger,
            CustomRouter router, IRouteStore store, IRouteSource source,
            IRoutingConfig config)
        {
            _router = router;
            _logger = logger?.CreateLogger<RoutingHost>();
            _store = store;
            _source = source;
            _config = config;

            var ignore = WatchLoop();
        }

        public CustomRouter Router => _router;

        private async Task WatchLoop()
        {
            while (_source.CanRequestRoute())
            {
                try
                {
                    var result = await _source.TryGetHealthChecksAsync();
                    if (!result.Success)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    await ProcessHealthChecksAsync(result.Checks);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(1000, ex, "There was an error getting available services from consul");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        private async Task ProcessHealthChecksAsync(HealthCheck[] healthChecks)
        {
            _logger?.LogInformation("Total number of health checks returned was {healthCheckCount}", healthChecks.Length);
            var infoList = BuildListOfHealthyServiceInstances(healthChecks);
            RemoveDeadInstances(infoList);
            var services = _store.GetServices();
            foreach (var service in services)
            {
                var infoService = await _source.GetServiceInstancesAsync(service.Key);
                foreach (var info in infoService)
                {
                    var instance = GetInstance(info, service.Value);
                    if (instance == null)
                    {
                        await CreateNewServiceInstanceAsync(service, info);
                    }
                    else
                    {
                        UpdateExistingRoutes(instance, info);
                    }
                }
            }
            _router.CleanUpRoutes();
            _config.OnRoutesBuilt?.Invoke(services.Keys.ToArray());
        }

        private async Task CreateNewServiceInstanceAsync(KeyValuePair<string, List<IService>> service, ServiceInstance info)
        {
            var instance = await _store.CreateServiceInstanceAsync(info);
            service.Value.Add(instance);
            _logger?.LogInformation("Adding a new service instance {serviceId} that is running the service {service} mapped to {routes}", instance.ServiceId, service.Key, instance.Routes);
            _router.AddNewService(instance);
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
            for (var i = 0; i < instanceList.Count; i++)
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
            foreach (var service in _store.GetServices().ToArray())
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
                    _store.RemoveService(service.Key);
                }
            }
            foreach (var i in infoList)
            {
                if (!_store.HasService(i.Service))
                {
                    _logger?.LogInformation("New service {serviceName} added because we have found instances of it", i.Service);
                    _store.AddService(i.Service);
                }
            }
        }

        private List<InformationService> BuildListOfHealthyServiceInstances(HealthCheck[] healthChecks)
        {
            var downNodes = new HashSet<string>();
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
            for (var i = 0; i < infoList.Count; i++)
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
