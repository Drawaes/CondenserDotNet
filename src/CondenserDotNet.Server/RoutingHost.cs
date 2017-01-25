﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CondenserDotNet.Core;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Server
{
    public class RoutingHost
    {
        private const string UrlPrefix = "urlprefix-";
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly CustomRouter _router;
        private readonly ILogger<RoutingHost> _logger;
        private readonly ServiceRegistry _services;

        private readonly Dictionary<string, Service> _servicesByName =
            new Dictionary<string, Service>();

        private readonly BlockingWatcher<Dictionary<string, string[]>> _watcher;

        public RoutingHost(CustomRouter router,
            CondenserConfiguration configuration, ILogger<RoutingHost> logger)
        {
            _router = router;
            _logger = logger;
            var uri = new Uri($"http://{configuration.AgentAddress}:{configuration.AgentPort}");
            var client = new HttpClient {BaseAddress = uri};

            _services = new ServiceRegistry(client, _cancel.Token);
            _watcher = new BlockingWatcher<Dictionary<string, string[]>>
                (HttpUtils.ServiceCatalogUrl, client, _cancel.Token, AddRemoveRoutes);
#pragma warning disable 4014
            _watcher.WatchLoop();
#pragma warning restore 4014
        }

        public IRouter Router => _router;

        public Action<Dictionary<string, string[]>> OnRouteBuilt { get; set; }

        private void AddRemoveRoutes(Dictionary<string, string[]> latestValues)
        {
            foreach (var server in latestValues)
            {
                var name = server.Key;
                var routes = FilterRoutes(server.Value);

                Service current;
                if (!_servicesByName.TryGetValue(name, out current))
                {
                    var service = new Service(routes,
                        name, name, server.Value, _services);

                    _servicesByName.Add(name, service);

                    _router.AddNewService(service);

                    _logger.LogInformation("Adding service {server}", service.ServiceId);
                }
                else
                {
                    var previousRoutes = current.Routes;

                    foreach (var newTag in routes.Except(previousRoutes))
                    {
                        _router.AddServiceToRoute(newTag, current);
                        _logger.LogInformation("Adding {route} to service {server}", newTag, server.Key);
                    }

                    foreach (var oldTag in previousRoutes.Except(routes))
                    {
                        _router.RemoveServiceFromRoute(oldTag, current);
                        _logger.LogInformation("Removing {route} from service {server}", oldTag, server.Key);
                    }
                }
            }

            foreach (var server in _servicesByName)
            {
                if (!latestValues.ContainsKey(server.Key))
                {
                    _router.RemoveService(server.Value);
                    _logger.LogInformation("Removing service {server}", server.Key);
                }
            }

            OnRouteBuilt?.Invoke(latestValues);
        }

        private static string[] FilterRoutes(string[] tags)
        {
            return tags
                .Where(x => x.StartsWith(UrlPrefix))
                .Select(x => x.Replace(UrlPrefix, ""))
                .ToArray();
        }
    }
}