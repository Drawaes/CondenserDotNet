using System;
using System.Net.Http;
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
        private readonly AsyncManualResetEvent<InformationService> _currentLeaderEvent = new AsyncManualResetEvent<InformationService>();
        private readonly AsyncManualResetEvent<bool> _electedLeaderEvent = new AsyncManualResetEvent<bool>();
        private readonly IServiceManager _serviceManager;
        private readonly string _keyToWatch;
        private Guid _sessionId;
        private Action<InformationService> _callback;
        private const string KeyPath = "/v1/kv/";
        private string _consulIndex = "0";

        internal LeaderWatcher(IServiceManager serviceManager, string keyToWatch)
        {
            _serviceManager = serviceManager;
            _keyToWatch = keyToWatch;
            var ignore = StartSession();
        }

        private async Task StartSession()
        {
            while (true)
            {
                _currentLeaderEvent.Reset();
                _electedLeaderEvent.Reset();
                var result = await _serviceManager.Client.PutAsync(HttpUtils.SessionCreateUrl, GetCreateSession());
                if (!result.IsSuccessStatusCode)
                {
                    await Task.Delay(1000);
                    continue;
                }
                _sessionId = JsonConvert.DeserializeObject<SessionCreateResponse>(await result.Content.ReadAsStringAsync()).Id;
                await TryForElection();
            }
        }

        private async Task TryForElection()
        {
            while (true)
            {
                //If we are here we don't know who is the leader
                _electedLeaderEvent.Reset();
                _currentLeaderEvent.Reset();
                var leaderResult = await _serviceManager.Client.PutAsync($"{KeyPath}{_keyToWatch}?acquire={_sessionId}", GetServiceInformation());
                if (!leaderResult.IsSuccessStatusCode)
                {
                    //error so we need to get a new session
                    await Task.Delay(500);
                    return;
                }
                var areWeLeader = bool.Parse(await leaderResult.Content.ReadAsStringAsync());
                if (areWeLeader)
                {
                    _electedLeaderEvent.Set(true);
                }
                for (var i = 0; i < 2; i++)
                {
                    leaderResult = await _serviceManager.Client.GetAsync($"{KeyPath}{_keyToWatch}?index={_consulIndex}");
                    if (!leaderResult.IsSuccessStatusCode)
                    {
                        //error so return to create session
                        return;
                    }
                    var kv = JsonConvert.DeserializeObject<KeyValue[]>(await leaderResult.Content.ReadAsStringAsync());
                    if (string.IsNullOrWhiteSpace(kv[0].Session))
                    {
                        _currentLeaderEvent.Reset();
                        _electedLeaderEvent.Reset();
                        break;
                    }
                    var infoService = JsonConvert.DeserializeObject<InformationService>(kv[0].ValueFromBase64());
                    _callback?.Invoke(infoService);
                    _currentLeaderEvent.Set(infoService);
                    if (Guid.Parse(kv[0].Session) == _sessionId)
                    {
                        _electedLeaderEvent.Set(true);
                    }
                    else
                    {
                        _electedLeaderEvent.Reset();
                    }
                    _consulIndex = leaderResult.GetConsulIndex();
                }
            }
        }

        private StringContent GetServiceInformation() =>
            HttpUtils.GetStringContent(new InformationService()
            {
                Address = _serviceManager.ServiceAddress,
                ID = _serviceManager.ServiceId,
                Port = _serviceManager.ServicePort,
                Service = _serviceManager.ServiceName,
                Tags = _serviceManager.RegisteredService.Tags.ToArray()
            });

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

        public Task<InformationService> GetCurrentLeaderAsync() => _currentLeaderEvent.WaitAsync();
        public Task GetLeadershipAsync() => _electedLeaderEvent.WaitAsync();
        public void SetLeaderCallback(Action<InformationService> callback) => _callback = callback;
    }
}
