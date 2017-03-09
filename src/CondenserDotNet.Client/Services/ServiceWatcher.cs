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
using Newtonsoft.Json;

namespace CondenserDotNet.Client.Services
{
    internal class ServiceWatcher:IDisposable
    {
        private readonly IRoutingStrategy<InformationServiceSet> _routingStrategy;
        private readonly ILogger _logger;
        private readonly string _serviceName;
        private readonly CancellationTokenSource _cancelationToken = new CancellationTokenSource();
        private readonly TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();
        private List<InformationServiceSet> _instances;
        private string _url;
        private WatcherState _state;
        private static int s_serviceReconnectDelay = 1500;
        private static int s_getServiceDelay = 3000;

        internal ServiceWatcher(string serviceName, HttpClient client,
            IRoutingStrategy<InformationServiceSet> routingStrategy, ILogger logger)
        {
            _serviceName = serviceName;
            _logger = logger;
            _routingStrategy = routingStrategy;
            _url = $"{HttpUtils.ServiceHealthUrl}{serviceName}?passing&index=";
            var ignore = WatcherLoop(client);
        }

        public WatcherState State => _state;

        internal async Task<InformationService> GetNextServiceInstanceAsync()
        {
            var instances = Volatile.Read(ref _instances);
            if (instances == null)
            {
                var delayTask = Task.Delay(s_getServiceDelay);
                var taskThatFinished = await Task.WhenAny(delayTask, _completionSource.Task).ConfigureAwait(false);
                if (delayTask == taskThatFinished)
                {
                    throw new NoConsulConnectionException();
                }
                instances = Volatile.Read(ref _instances);
            }
            if (_state != WatcherState.UsingLiveValues)
            {
                _logger?.LogWarning("Using old values for service {serviceList}", instances);
            }
            return _routingStrategy.RouteTo(instances)?.Service;
        }

        private async Task WatcherLoop(HttpClient client)
        {
            while (true)
            {
                try
                {
                    string consulIndex = "0";
                    while (!_cancelationToken.Token.IsCancellationRequested)
                    {
                        var result = await client.GetAsync(_url + consulIndex, _cancelationToken.Token);
                        if (!result.IsSuccessStatusCode)
                        {
                            if (_state == WatcherState.UsingLiveValues)
                            {
                                _state = WatcherState.UsingCachedValues;
                            }
                            await Task.Delay(1000);
                            continue;
                        }
                        consulIndex = result.GetConsulIndex();
                        var content = await result.Content.ReadAsStringAsync();
                        var instance = JsonConvert.DeserializeObject<List<InformationServiceSet>>(content);
                        Volatile.Write(ref _instances, instance);
                        _state = WatcherState.UsingLiveValues;
                        _completionSource.TrySetResult(true);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(0, ex, "Error in blocking watcher watching consul");
                }
                _state = _state == WatcherState.NotInitialized ? WatcherState.NotInitialized : WatcherState.UsingCachedValues;
                if (_cancelationToken.Token.IsCancellationRequested)
                {
                    _logger?.LogInformation("Cancelation requested exiting watcher");
                    return;
                }
                await Task.Delay(s_serviceReconnectDelay);
            }
        }

        public void Dispose()
        {
            _cancelationToken.Cancel();
        }
    }
}