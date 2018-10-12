using System;
using System.Collections.Generic;
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
    internal class ServiceWatcher : IDisposable
    {
        private IRoutingStrategy<InformationServiceSet> _routingStrategy;
        private readonly ILogger _logger;
        private readonly string _serviceName;
        private readonly CancellationTokenSource _cancelationToken = new CancellationTokenSource();
        private readonly TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();
        private List<InformationServiceSet> _instances;
        private readonly string _url;
        private WatcherState _state;
        private static readonly int s_serviceReconnectDelay = 1500;
        private static readonly int s_getServiceDelay = 6000;
        private Action<List<InformationServiceSet>> _listCallback;
        private bool _isNearest;

        internal ServiceWatcher(string serviceName, HttpClient client, IRoutingStrategy<InformationServiceSet> routingStrategy, ILogger logger, bool useNearest)
        {
            _isNearest = useNearest;
            _serviceName = serviceName;
            _logger = logger;
            _routingStrategy = routingStrategy;
            _url = $"{HttpUtils.ServiceHealthUrl}{serviceName}?passing=true";
            if (_isNearest)
            {
                _url += "&near=_agent";
            }
            _url += "&index=";
            _logger?.LogInformation("Started watching service with name {serviceName}", _serviceName);
            var ignore = WatcherLoop(client);
        }

        public bool IsNearest { get => _isNearest; set => _isNearest = value; }
        public WatcherState State => _state;
        public IRoutingStrategy<InformationServiceSet> RoutingStrategy { get => _routingStrategy; set => _routingStrategy = value; }

        public void SetCallback(Action<List<InformationServiceSet>> callBack)
        {
            _logger?.LogTrace("Logging callback for {serviceName}", _serviceName);
            _listCallback = callBack;
            _listCallback?.Invoke(_instances);
        }

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
            var serviceInstance = _routingStrategy.RouteTo(instances)?.Service;
            if (serviceInstance == null)
            {
                _logger?.LogWarning("No service instance was found for service name {serviceName}", _serviceName);
            }
            return serviceInstance;
        }

        private async Task WatcherLoop(HttpClient client)
        {
            try
            {
                while (true)
                {
                    try
                    {
                        var consulIndex = "0";
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
                            var newConsulIndex = result.GetConsulIndex();
                            if (newConsulIndex == consulIndex)
                            {
                                continue;
                            }
                            consulIndex = newConsulIndex;
                            var content = await result.Content.ReadAsStringAsync();
                            var instance = JsonConvert.DeserializeObject<List<InformationServiceSet>>(content);
                            Volatile.Write(ref _instances, instance);
                            _listCallback?.Invoke(instance);
                            _state = WatcherState.UsingLiveValues;
                            _completionSource.TrySetResult(true);
                            continue;
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
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "Exception in watcher loop for {serviceName}", _serviceName);
            }
            finally
            {
                _logger?.LogWarning("Exiting watcher loop for {serviceName}", _serviceName);
            }
        }

        public void Dispose() => _cancelationToken.Cancel();
    }
}
