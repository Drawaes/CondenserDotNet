using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Server.DataContracts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CondenserDotNet.Server
{
    public class RoutingHost
    {
        private Dictionary<string, List<Service>> _servicesWithHealthChecks = new Dictionary<string, List<Service>>();
        private readonly string _healthCheckUri;
        private readonly string _serviceLookupUri;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly CustomRouter _router;
        private readonly CondenserConfiguration _config;
        private readonly HttpClient _client = new HttpClient();

        public RoutingHost(CustomRouter router,
            CondenserConfiguration configuration, ILogger<RoutingHost> logger)
        {
            _config = configuration;
            _router = router;
            _healthCheckUri = $"http://{_config.AgentAddress}:{_config.AgentPort}{HttpUtils.HealthAnyUrl}?index=";
            _serviceLookupUri = $"http://{_config.AgentAddress}:{_config.AgentPort}{HttpUtils.SingleServiceCatalogUrl}";
            WatchLoop();
        }

        public IRouter Router => _router;
        public Action<Dictionary<string, List<Service>>> OnRouteBuilt { get; set; }

        private async void WatchLoop()
        {
            string index = string.Empty;
            while (!_cancel.IsCancellationRequested)
            {
                var result = await _client.GetAsync(_healthCheckUri + index);
                if (!result.IsSuccessStatusCode)
                {
                    //need to log here
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }
                index = result.GetConsulIndex();
                // Log stuff about updating routing
                var content = await result.Content.ReadAsStringAsync();
                var healthChecks = JsonConvert.DeserializeObject<HealthCheck[]>(content);
                List<InformationService> infoList = BuildListOfHealthyServiceInstances(healthChecks);
                RemoveDeadInstances(infoList);
                foreach (var service in _servicesWithHealthChecks)
                {
                    result = await _client.GetAsync(_serviceLookupUri + service.Key);
                    content = await result.Content.ReadAsStringAsync();
                    var infoService = JsonConvert.DeserializeObject<ServiceInstance[]>(content);
                    foreach (var info in infoService)
                    {
                        var instance = GetInstance(info, service.Value);
                        if (instance == null)
                        {
                            instance = new Service(info.ServiceID, info.Node, info.ServiceTags, info.ServiceAddress, info.ServicePort);
                            _router.AddNewService(instance);
                            continue;
                        }
                        var routes = Service.RoutesFromTags(info.ServiceTags);

                        if (instance.Routes.SequenceEqual(routes))
                        {
                            continue;
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
                }
                OnRouteBuilt?.Invoke(_servicesWithHealthChecks);
            }
        }

        private Service GetInstance(ServiceInstance service, List<Service> instanceList)
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
            foreach (var service in _servicesWithHealthChecks.ToArray())
            {
                foreach (var instance in service.Value.ToArray())
                {
                    if (!HasInstance(instance.ServiceId, infoList))
                    {
                        //Service is dead remove it
                        service.Value.Remove(instance);
                        _router.RemoveService(instance);
                    }
                }
                if (service.Value.Count == 0)
                {
                    _servicesWithHealthChecks.Remove(service.Key);
                }
            }
            foreach (var i in infoList)
            {
                if (!_servicesWithHealthChecks.ContainsKey(i.Service))
                {
                    _servicesWithHealthChecks[i.Service] = new List<Service>();
                }
            }
        }

        private static List<InformationService> BuildListOfHealthyServiceInstances(HealthCheck[] healthChecks)
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
