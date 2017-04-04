﻿using CondenserDotNet.Client;
using CondenserDotNet.Core;
using CondenserDotNet.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Condenser.Tests.Integration.Routing
{
    public class RoutingFixture : IDisposable
    {
        private Dictionary<string, ServiceInstance> _hosts = new Dictionary<string, ServiceInstance>();
        private const string HealthRoute = "/health";
        private volatile string[] _currentRegistrations;
        private int routerPort;
        private AsyncManualResetEvent<bool> _wait = new AsyncManualResetEvent<bool>();
        private RoutingHost _host;
        private IWebHost _routerHost;
        private HttpClient _client = new HttpClient();

        public RoutingFixture()
        {
            Console.WriteLine("Created Routing Fixture");
        }

        public void SetServiceHealth(string name, bool isHealthy)
        {
            _hosts[name].IsHealthy = isHealthy;
        }

        public string GetNewServiceName()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        public Task<HttpResponseMessage> CallRouterAsync(string route)
        {
            return _client.GetAsync($"http://localhost:{routerPort}" + route);
        }

        private void RegisterService(string name, int port, string route)
        {
            var options = Options.Create(new ServiceManagerConfig
            {
                ServiceName = name,
                ServicePort = port

            });

            var serviceManager = new ServiceManager(options);

            var ignore = serviceManager
                .AddHttpHealthCheck(HealthRoute, 1)
                .AddApiUrl(route)
                .RegisterServiceAsync();
        }

        public RoutingFixture AddService(string name, string route)
        {
            var hostPort = ServiceManagerConfig.GetNextAvailablePort();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{hostPort}")
                .Configure(app =>
                {
                    RegisterService(name, hostPort, route);

                    app.Use(async (context, next) =>
                    {
                        try
                        {
                            await next();
                        }
                        catch(Exception ex)
                        {
                           throw;
                        }
                    });

                    app.Run(async message =>
                    {
                        HttpStatusCode status;
                        string content = null;
                        var path = message.Request.Path;

                        var instance = _hosts[name];

                        if (path == HealthRoute)
                        {
                            if (instance.IsHealthy)
                            {
                                status = HttpStatusCode.OK;
                                content = "Healthy";
                            }
                            else
                            {
                                status = HttpStatusCode.InternalServerError;
                                content = "Not healthy";
                            }
                        }
                        else if (path == route)
                        {
                            status = HttpStatusCode.OK;
                            content = "Called me " + name;
                        }
                        else
                        {
                            status = HttpStatusCode.NotFound;
                            content = "";
                        }

                        message.Response.StatusCode = (int)status;
                        await message.Response.WriteAsync(content);

                    });
                })
                .Build();

            _hosts.Add(name, new ServiceInstance(host));

            return this;
        }

        public void AddRouter()
        {
            routerPort = ServiceManagerConfig.GetNextAvailablePort();

            _routerHost = new WebHostBuilder()
                .UseKestrel()
                .UseLoggerFactory(new LoggerFactory().AddConsole())
                .UseUrls($"http://*:{routerPort}")
                .ConfigureServices(x =>
                {
                    x.AddCondenserWithBuilder()
                    .WithRoutesBuiltCallback(SignalWhenAllRegistered)                    
                    .Build();
                    
                })
                .Configure(app =>
                {
                    _host = app.ApplicationServices.GetService<RoutingHost>();


                    app.UseCondenser();
                })
                .Build();


        }

        public Task WaitForRegistrationAsync()
        {
            return Task.WhenAny(new[] { _wait.WaitAsync(), Task.Delay(30 * 1000) });
        }

        private bool AllRegistered(string[] data)
        {
            return _hosts.All(h => data.Contains(h.Key, StringComparer.OrdinalIgnoreCase));
        }

        public bool AreAllRegistered()
        {
            if (_currentRegistrations == null)
                return false;

            return AllRegistered(_currentRegistrations);
        }

        private void SignalWhenAllRegistered(string[] data)
        {
            Interlocked.Exchange(ref _currentRegistrations, data);

            if (AllRegistered(data))
            {
                _wait.Set(true);
            }           
        }

        public void StartAll()
        {
            foreach (var host in _hosts)
            {
                host.Value.Host.Start();
            }

            _routerHost?.Start();
        }

        public void Dispose()
        {
            foreach (var host in _hosts)
            {
                host.Value.Host.Dispose();
            }

            _routerHost?.Dispose();
        }

        public class ServiceInstance
        {
            public IWebHost Host { get; }

            public bool IsHealthy { get; set; }
            public ServiceInstance(IWebHost host)
            {
                Host = host;
                IsHealthy = true;
            }
        }
    }
}
