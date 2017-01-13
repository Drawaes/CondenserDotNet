using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Service.DataContracts;
using Newtonsoft.Json;

namespace CondenserDotNet.Service
{
    internal class ServiceWatcher
    {
        //used to randomly select an instance
        static int seed = Environment.TickCount;
        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        private readonly AsyncManualResetEvent<bool> _haveFirstResults = new AsyncManualResetEvent<bool>();

        private readonly string _serviceName;
        private readonly HttpClient _client;
        private readonly CancellationToken _cancel;
        private readonly string _lookupUrl;
        private InformationServiceSet[] _serviceInstances;
        private WatcherState _state = WatcherState.NotInitialized;

        public ServiceWatcher(string serviceName, 
            HttpClient client, CancellationToken cancel)
        {
            _serviceName = serviceName;
            _client = client;
            _cancel = cancel;
            _lookupUrl = $"{HttpUtils.ServiceHealthUrl}{serviceName}?passing&index=";
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            WatchLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        internal async Task<InformationService> GetNextServiceInstanceAsync()
        {
            if (!await _haveFirstResults.WaitAsync())
            {
                return null;
            }
            InformationServiceSet[] instances = Volatile.Read(ref _serviceInstances);
            if (instances.Length > 0)
            {
                return instances[random.Value.Next(0, instances.Length)].Service;
            }
            return null;
        }

        private async Task WatchLoop()
        {
            try
            {
                string consulIndex = "0";
                while (true)
                {
                    var result = await _client.GetAsync(_lookupUrl + consulIndex, 
                        _cancel);
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
                    var listOfServices = JsonConvert.DeserializeObject<InformationServiceSet[]>(content);
                    Interlocked.Exchange(ref _serviceInstances, listOfServices);
                    _state = WatcherState.UsingLiveValues;
                    _haveFirstResults.Set(true);
                }
            }
            catch (TaskCanceledException) { /*nom nom */}
            catch (ObjectDisposedException) { /*nom nom */}
        }

        public enum WatcherState
        {
            NotInitialized,
            UsingCachedValues,
            UsingLiveValues
        }
    }
}
