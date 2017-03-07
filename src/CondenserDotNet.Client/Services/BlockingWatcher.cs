using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core;
using Newtonsoft.Json;

namespace CondenserDotNet.Client.Services
{
    internal class BlockingWatcher<T> where T : class
    {
        private readonly AsyncManualResetEvent<bool> _haveFirstResults = new AsyncManualResetEvent<bool>();
        private readonly Func<string, Task<HttpResponseMessage>> _client;
        private readonly Action<T> _onNew;
        private T _instances;
        private WatcherState _state = WatcherState.NotInitialized;
        private static int s_getServiceDelay = System.Diagnostics.Debugger.IsAttached ? 30000 : 2000;
        
        public BlockingWatcher(Func<string, Task<HttpResponseMessage>> client, Action<T> onNew = null)
        {
            _client = client;
            _onNew = onNew;
        }

        public async Task<T> ReadAsync()
        {
            var delayTask = Task.Delay(s_getServiceDelay);
            var taskThatFinished = await Task.WhenAny(delayTask,_haveFirstResults.WaitAsync());
            if (delayTask == taskThatFinished)
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
                    _onNew?.Invoke(instance);
                }
            }
            catch (TaskCanceledException) { /*nom nom */}
            catch (ObjectDisposedException) { /*nom nom */}
        }
    }
}