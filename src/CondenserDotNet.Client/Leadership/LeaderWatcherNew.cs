using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Consul;
using CondenserDotNet.Core.DataContracts;
using Newtonsoft.Json;

namespace CondenserDotNet.Client.Leadership
{
    public class LeaderWatcher : ILeaderWatcher
    {
        private readonly AsyncManualResetEvent<bool> _electedLeaderEvent = new AsyncManualResetEvent<bool>();
        private Action<InformationService> _callback;
        private const string KeyPath = "/v1/kv/";
        private string _consulIndex = "0";
        private Task<Guid> _sessionIdTask;
        private IServiceManager _serviceManager;
        private string _keyToWatch;
        private AsyncManualResetEvent<InformationService> _currentInfoService = new AsyncManualResetEvent<InformationService>();
        
        public LeaderWatcher(IServiceManager serviceManager, string keyToWatch)
        {
            _currentInfoService.Reset();
            _electedLeaderEvent.Reset();
            _keyToWatch = keyToWatch;
            _serviceManager = serviceManager;
            _sessionIdTask = GetSession();
            var ignore = KeepLeadershipLoop();
        }

        private async Task<Guid> GetSession()
        {
            while (true)
            {
                CondenserEventSource.Log.LeadershipSessionCreated();
                var result = await _serviceManager.Client.PutAsync(HttpUtils.SessionCreateUrl, GetCreateSession());
                if (!result.IsSuccessStatusCode)
                {
                    await Task.Delay(1000);
                    continue;
                }
                return JsonConvert.DeserializeObject<SessionCreateResponse>(await result.Content.ReadAsStringAsync()).Id;
            }
        }

        private StringContent GetCreateSession()
        {
            var checks = new string[_serviceManager.RegisteredService.Checks.Count + 1];
            for (var i = 0; i < _serviceManager.RegisteredService.Checks.Count; i++)
            {
                checks[i] = _serviceManager.RegisteredService.Checks[i].Name;
            }
            checks[checks.Length - 1] = "serfHealth";
            var sessionCreate = new SessionCreate()
            {
                Behavior = "delete",
                Checks = checks,
                LockDelay = "1s",
                Name = $"{_serviceManager.ServiceId}:LeaderElection:{_keyToWatch.Replace('/', ':')}"
            };
            return HttpUtils.GetStringContent(sessionCreate);
        }

        private StringContent GetServiceInformation() => HttpUtils.GetStringContent(new InformationService()
        {
            Address = _serviceManager.ServiceAddress,
            ID = _serviceManager.ServiceId,
            Port = _serviceManager.ServicePort,
            Service = _serviceManager.ServiceName,
            Tags = _serviceManager.RegisteredService.Tags.ToArray()
        });

        private async Task KeepLeadershipLoop()
        {
            var sessionId = await _sessionIdTask;
            while (true)
            {
                var leaderResult = await _serviceManager.Client.PutAsync($"{KeyPath}{_keyToWatch}?acquire={sessionId}", GetServiceInformation());
                if (!leaderResult.IsSuccessStatusCode)
                {
                    //error so we need to get a new session
                    await Task.Delay(500);
                    continue;
                }
                var areWeLeader = bool.Parse(await leaderResult.Content.ReadAsStringAsync());
                if(areWeLeader)
                {
                    _electedLeaderEvent.Set(true);
                }
                else
                {
                    _electedLeaderEvent.Reset();
                }
                await WaitForLeadershipChange();
            }
        }

        private async Task WaitForLeadershipChange()
        {
            while(true)
            {
                using (var leaderResult = await _serviceManager.Client.GetAsync($"{KeyPath}{_keyToWatch}?index={_consulIndex}"))
                {
                    if (!leaderResult.IsSuccessStatusCode)
                    {
                        //Lock deleted
                        if (leaderResult.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            _electedLeaderEvent.Reset();
                            _currentInfoService.Reset();
                            return;
                        }
                        await Task.Delay(500);
                        continue;
                    }
                    var kv = JsonConvert.DeserializeObject<KeyValue[]>(await leaderResult.Content.ReadAsStringAsync());
                    if (string.IsNullOrWhiteSpace(kv[0].Session))
                    {
                        //no one has leadership
                        _currentInfoService.Reset();
                        _electedLeaderEvent.Reset();
                        return;
                    }
                    var infoService = JsonConvert.DeserializeObject<InformationService>(kv[0].ValueFromBase64());
                    _currentInfoService.Set(infoService);
                    _consulIndex = leaderResult.GetConsulIndex();
                    _callback?.Invoke(infoService);
                    if (await _sessionIdTask != new Guid(kv[0].Session))
                    {
                        _electedLeaderEvent.Reset();
                        return;
                    }
                }
            }
        }

        public async Task<InformationService> GetCurrentLeaderAsync()
        {
            await _sessionIdTask;
            return await _currentInfoService.WaitAsync();
        }

        public async Task GetLeadershipAsync()
        {
            await _sessionIdTask;
            await _electedLeaderEvent.WaitAsync();
        }

        public void SetLeaderCallback(Action<InformationService> callback) => _callback = callback;
    }
}
