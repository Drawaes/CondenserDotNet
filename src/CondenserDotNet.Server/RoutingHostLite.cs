using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Server.DataContracts;
using CondenserDotNet.Service;
using CondenserDotNet.Service.DataContracts;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace CondenserDotNet.Server
{
    public class RoutingHostLite
    {
        private Dictionary<string, List<Service>> _servicesWithHealthChecks = new Dictionary<string, List<Service>>();
        private const string UrlPrefix = "urlprefix-";
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly CustomRouter _router;
        private readonly CondenserConfiguration _config;
        private readonly HttpClient _client = new HttpClient();

        public RoutingHostLite(CustomRouter router,
            CondenserConfiguration configuration)
        {
            _config = configuration;
            _router = router;
            WatchLoop();
        }

        public IRouter Router => _router;

        private async void WatchLoop()
        {
            string index = string.Empty;
            while (!_cancel.IsCancellationRequested)
            {
                var result = await _client.GetAsync($"http://{_config.AgentAddress}:{_config.AgentPort}{HttpUtils.HealthAnyUrl}?index={index}");
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
                foreach(var service in _servicesWithHealthChecks)
                {
                    result = await _client.GetAsync($"http://{_config.AgentAddress}:{_config.AgentPort}{HttpUtils.SingleServiceCatalogUrl}{service.Key}");
                    content = await result.Content.ReadAsStringAsync();
                    var infoService = JsonConvert.DeserializeObject<ServiceInstance[]>(content);
                    foreach(var info in infoService)
                    {
                        var instance = GetInstance(info, service.Value);
                        if(instance == null)
                        {
                            instance = new Service(FilterRoutes(info.ServiceTags), info.ServiceID, info.Node, info.ServiceTags,info.ServiceAddress, info.ServicePort);
                            _router.AddNewService(instance);
                            continue;
                        }
                        var routes = FilterRoutes(info.ServiceTags);
                        routes = routes.Select(r => !r.StartsWith("/") ? "/" + r : r).Select(r => r.EndsWith("/") ? r.Substring(0, r.Length - 1) : r).ToArray();

                        if (instance.Routes.SequenceEqual(routes))
                        {
                            continue;
                        }

                        foreach (var newTag in routes.Except(instance.Routes))
                            _router.AddServiceToRoute(newTag, instance);

                        foreach (var oldTag in instance.Routes.Except(routes))
                            _router.RemoveServiceFromRoute(oldTag, instance);
                        instance.UpdateRoutes(routes);
                    }
                }
            }
        }

        private static string[] FilterRoutes(string[] tags)
        {
            return tags
                .Where(x => x.StartsWith(UrlPrefix))
                .Select(x => x.Replace(UrlPrefix, ""))
                .ToArray();
        }

        private Service GetInstance(ServiceInstance service, List<Service> instanceList)
        {
            for(int i = 0; i < instanceList.Count;i++)
            {
                if(instanceList[i].ServiceId == service.ServiceID)
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
                if(service.Value.Count == 0)
                {
                    _servicesWithHealthChecks.Remove(service.Key);
                }
            }
            foreach(var i in infoList)
            {
                if(!_servicesWithHealthChecks.ContainsKey(i.Service))
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
