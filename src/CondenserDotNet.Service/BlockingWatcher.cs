using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CondenserDotNet.Service
{
    internal class BlockingWatcher<T>
        where T : class
    {
        
        private readonly AsyncManualResetEvent<bool> _haveFirstResults = new AsyncManualResetEvent<bool>();

        private readonly HttpClient _client;
        private readonly CancellationToken _cancel;
        private readonly string _lookupUrl;
        private T _serviceInstances;
        private WatcherState _state = WatcherState.NotInitialized;

        public BlockingWatcher(string url,
            HttpClient client, CancellationToken cancel)
        {
            _client = client;
            _cancel = cancel;
            _lookupUrl = $"{url}?passing&index=";
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            WatchLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        internal async Task<T> SafeReadAsync()
        {
            if (!await _haveFirstResults.WaitAsync())
            {
                return null;
            }
            T instances = Volatile.Read(ref _serviceInstances);
            return instances;
        }

        internal async Task WatchLoop()
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
                    var listOfServices = JsonConvert.DeserializeObject<T>(content);
                    Interlocked.Exchange(ref _serviceInstances, listOfServices);
                    _state = WatcherState.UsingLiveValues;
                    _haveFirstResults.Set(true);
                }
            }
            catch (TaskCanceledException) { /*nom nom */}
            catch (ObjectDisposedException) { /*nom nom */}
        }
    }
}