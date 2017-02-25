using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Services.Internal;
using Newtonsoft.Json;

namespace CondenserDotNet.Services.Consul
{
    public class BlockingWatcher<T> where T : class
    {
        private readonly AsyncManualResetEvent<bool> _haveFirstResults = new AsyncManualResetEvent<bool>();
        private readonly HttpClient _client;
        private readonly CancellationToken _cancel;
        private readonly Action<T> _onNew;
        private readonly string _lookupUrl;
        private T _instances;
        private WatcherState _state = WatcherState.NotInitialized;

        public BlockingWatcher(string url, HttpClient client, CancellationToken cancel, Action<T> onNew = null)
        {
            _client = client;
            _cancel = cancel;
            _onNew = onNew;
            _lookupUrl = $"{url}?passing&index=";
        }

        public async Task<T> ReadAsync()
        {
            if (!await _haveFirstResults.WaitAsync())
            {
                return null;
            }
            T instances = Volatile.Read(ref _instances);
            return instances;
        }

        public async Task WatchLoop()
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
                    var instance = JsonConvert.DeserializeObject<T>(content);
                    Interlocked.Exchange(ref _instances, instance);
                    _state = WatcherState.UsingLiveValues;
                    _haveFirstResults.Set(true);
                    _onNew?.Invoke(instance);
                }
            }
            catch (TaskCanceledException) { /*nom nom */}
            catch (ObjectDisposedException) { /*nom nom */}
        }
    }
}