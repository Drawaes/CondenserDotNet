using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Client.Services
{
    internal class BlockingWatcher<T> where T : class
    {
        private readonly AsyncManualResetEvent<bool> _haveFirstResults = new AsyncManualResetEvent<bool>();
        private readonly Func<string, Task<HttpResponseMessage>> _client;
        private T _instances;
        private WatcherState _state = WatcherState.NotInitialized;
        private static int s_getServiceDelay = 2000;
        private readonly ILogger _logger;
        private CancellationToken _token;

        public BlockingWatcher(Func<string, Task<HttpResponseMessage>> client, ILogger logger, CancellationToken token)
        {
            _token = token;
            _client = client;
            _logger = logger;
        }

        public async Task<T> ReadAsync()
        {
            T instances = Volatile.Read(ref _instances);
            if (instances == null)
            {
                var delayTask = Task.Delay(s_getServiceDelay);
                var taskThatFinished = await Task.WhenAny(delayTask, _haveFirstResults.WaitAsync());
                if (delayTask == taskThatFinished)
                {
                    throw new NoConsulConnectionException();
                }
                instances = Volatile.Read(ref _instances);
            }
            return instances;
        }

        public async Task WatchLoop()
        {
            while (true)
            {
                try
                {
                    string consulIndex = "0";
                    while (!_token.IsCancellationRequested)
                    {
                        var result = await _client(consulIndex);
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
                    }
                }
                catch (TaskCanceledException) { /*nom nom */}
                catch (ObjectDisposedException) { /*nom nom */}
                catch (Exception ex)
                {
                    _logger?.LogWarning(0, ex, "Error in blocking watcher watching consul");
                }
                await Task.Delay(s_getServiceDelay);
            }
        }
    }
}