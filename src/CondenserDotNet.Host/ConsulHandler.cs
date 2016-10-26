using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CondenserDotNet.Host
{
    public class ConsulHandler
    {
        //ConsulClient _consul;
        //QueryResult<Dictionary<string,string[]>> _lastServiceCall;
        //Task _runConsul;
        private CustomRouter _router;
        HttpClient _client;
        Uri _healthCheckUri;
        //Dictionary<string, Service> _currentServices = new Dictionary<string, Service>();

        public ConsulHandler(CustomRouter router)
        {
            _healthCheckUri = new Uri("localhost:8500/v1/health/state/any");
            _client = new HttpClient();
            _router = router;
            KeepCheckingConsul();
        }

        private async void KeepCheckingConsul()
        {
            var queryResult = await _client.GetStringAsync(_healthCheckUri);
            //QueryOptions qOptions = new QueryOptions();
            //qOptions.WaitTime = new TimeSpan(0, 30, 0);
            //while (true)
            //{
                
            //    HashSet<string> _listOfCurrentServiceIds = new HashSet<string>(_currentServices.Keys);
            //    foreach (var s in _lastServiceCall.Response)
            //    {
            //        if(s.Value.Any(v => v.StartsWith("url=")))
            //        {
            //            var serviceInfo = _consul.Health.Service(s.Key).Result;

            //            foreach(var catService in serviceInfo.Response)
            //            {
            //                if(catService.Service.Tags.Any(v => v.StartsWith("url=")) && catService.Checks.All(v => v.Status == "passing"))
            //                {
            //                    string[] routes = catService.Service.Tags.Where(t => t.StartsWith("url=")).Select(t => t.Substring(4)).ToArray();
            //                    Service currentService;
            //                    if(_currentServices.TryGetValue(catService.Service.ID, out currentService))
            //                    {
            //                        //We already have it addressed so lets see how it has changed
            //                        if(!currentService.Equals(catService))
            //                        {
            //                            _router.RemoveService(currentService);
            //                            //it has changed address or port so we need to create a new service completely
            //                            //so we will let it fall through
            //                            _listOfCurrentServiceIds.Remove(currentService.ServiceId);
            //                        }
            //                        else
            //                        {
            //                            ///Adress hasn't changed so we need to see if any routes have been added or removed
            //                            var routesRemoved = currentService.Routes.Where(oldRoute => ! routes.Contains(oldRoute));
            //                            var routesAdded = routes.Where(newRoute => !currentService.Routes.Contains(newRoute));
            //                            foreach(var r in routesRemoved)
            //                            {
            //                                _router.RemoveServiceFromRoute(r, currentService);
            //                            }
            //                            foreach(var r in routesAdded)
            //                            {
            //                                _router.AddServiceToRoute(r, currentService);
            //                            }
            //                            currentService.UpdateRoutes(routes);
            //                            _listOfCurrentServiceIds.Remove(currentService.ServiceId);
            //                        }
            //                    }
            //                    currentService = new Service(routes, catService.Service.ID, catService.Service.Address, catService.Service.Port, catService.Service.Tags.Where(t => !t.StartsWith("url=")).ToArray());
            //                    _router.AddNewService(currentService);
            //                    _currentServices[currentService.ServiceId] = currentService;
            //                }
            //            }
            //        }
            //    }
            //    //Finally any services left aren't in the catalog anymore... goodbye!
            //    foreach(var sId in _listOfCurrentServiceIds)
            //    {
            //        var service = _currentServices[sId];
            //        _router.RemoveService(service);
            //        _currentServices.Remove(sId);
            //    }

            //    //Clean up the tree
            //    _router.CleanUpRoutes();

            //    qOptions.WaitIndex = _lastServiceCall.LastIndex;
            //    _lastServiceCall = _consul.Catalog.Services(qOptions).Result;
            //}
        }
    }
}
