/*Thanks and credit to the idea of a reseting awaitor goes to Marc Gravel and his work on channels*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class Leader : ClientBase, INotifyCompletion
    {
        private Guid _sessionKey = Guid.NewGuid();
        private string _sessionCreateString;
        private static readonly string _sessionCreateQueryString = "/v1/session/create";
        private string _keyname;
        private string _serviceId;
        private CancellationToken _cancel = new CancellationToken(false);
        private ManualResetEvent _isLeader = new ManualResetEvent(false);
        private Action _continuation;

        public Leader(string keyname, string serviceId)
        {
            _serviceId = serviceId;
            _keyname = keyname;

            var sessionCreate = new SessionCreate()
            {
                Behavior = "release",
                Checks = new string[] { "serfHealth", $"service:{serviceId}" },
                LockDelay = "0s",
                Name = $"{_serviceId}:LeaderElection",
            };
            _sessionCreateString = JsonConvert.SerializeObject(sessionCreate);

            StartSession();
        }

        public bool IsCompleted => _isLeader.WaitOne(0);
        // utility method for people who don't feel comfortable with `await obj;` and prefer `await obj.WaitAsync();`
        public Leader WaitAsync() => this;
        public void GetResult() { }
        public Leader GetAwaiter() => this;

        private void Set()
        {
            _isLeader.Set();
            var continuation = Interlocked.Exchange(ref _continuation,null);
            if (continuation != null)
            {
                ThreadPool.QueueUserWorkItem(state => ((Action)state).Invoke(), continuation);
            }
        }

        private void Reset()
        {
            _isLeader.Reset();
        }

        private async void StartSession()
        {
            HttpResponseMessage sessionCreateReturn;
            while (true)
            {
                var createSessionPayload = new StringContent(_sessionCreateString, System.Text.Encoding.UTF8, "application/json");
                sessionCreateReturn = await _httpClient.PutAsync(_sessionCreateQueryString, createSessionPayload);
                if (sessionCreateReturn.IsSuccessStatusCode)
                {
                    //Now try to get the lock
                    var sessionResponse = JsonConvert.DeserializeObject<SessionCreateResponse>(await sessionCreateReturn.Content.ReadAsStringAsync());
                    await TryToBecomeLeader(sessionResponse.Id);
                }
                //Failed to get a session, probably because our health checks are failing, so back off and try again
                await Task.Delay(500);
            }
        }

        private async Task TryToBecomeLeader(string sessionId)
        {
            while (true)
            {
                string currentIndex = null;
                var putSession = new StringContent(_serviceId, System.Text.Encoding.UTF8);
                var lockResponse = await _httpClient.PutAsync($"/v1/kv/{_keyname}?acquire={sessionId}", putSession, _cancel);
                if (!lockResponse.IsSuccessStatusCode)
                {
                    //We got an error, we need to reaquire our session at this point
                    Reset();
                    break;
                }
                var responseValue = bool.Parse(await lockResponse.Content.ReadAsStringAsync());
                if(responseValue)
                {
                    Set();
                }
                else
                {
                    Reset();
                }

                while (true)
                {
                    string queryString = $"/v1/kv/{_keyname}?" + currentIndex != null ? $"index={currentIndex}&wait=300s" : "";
                    //Now get the key info
                    var getResponse = await _httpClient.GetAsync(queryString, _cancel);
                    var keyString = await getResponse.Content.ReadAsStringAsync();
                    var keyInfo = JsonConvert.DeserializeObject<FullKeyInfo[]>(keyString)?.FirstOrDefault();
                    IEnumerable<string> waitTime = null;
                    getResponse.Headers.TryGetValues(ConsulIndexHeader, out waitTime);
                    currentIndex = waitTime?.FirstOrDefault();
                    if (keyInfo?.Session == sessionId)
                    {
                        Set();
                    }
                    else
                    {
                        Reset();
                        if (string.IsNullOrWhiteSpace(keyInfo?.Session))
                        {
                            //No one owns it so try again
                            break;
                        }
                    }
                }
            }
        }

        public void OnCompleted(Action continuation)
        {
            if (continuation != null)
            {
                if (_isLeader.WaitOne(0))
                {
                    // already complete; callback sync
                    continuation.Invoke();
                    return;
                }
                else
                {
                    Volatile.Write(ref _continuation, continuation);
                }
            }
        }
    }
}
