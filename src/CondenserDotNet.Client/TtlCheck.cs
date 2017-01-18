using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;

namespace CondenserDotNet.Client
{
    public class TtlCheck : ITtlCheck
    {
        private IServiceManager _parentManager;
        private int _timeToLiveSeconds;
        private HealthCheck _healthCheck;

        internal TtlCheck(IServiceManager parentManager, int timeToLiveSeconds)
        {
            _parentManager = parentManager;
            _healthCheck = new HealthCheck()
            {
                TTL = $"{timeToLiveSeconds}s"
            };
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public HealthCheck HealthCheck => _healthCheck;

        public async Task<bool> ReportPassingAsync()
        {
            if(!_parentManager.IsRegistered)
            {
                return false;
            }
            //We are connected so lets report to the server
            var response = await _parentManager.Client.GetAsync($"/v1/agent/check/pass/service:{_parentManager.ServiceId}");
            if(response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> ReportWarningAsync()
        {
            if (!_parentManager.IsRegistered)
            {
                return false;
            }
            //We are connected so lets report to the server
            var response = await _parentManager.Client.GetAsync($"/v1/agent/check/warn/service:{_parentManager.ServiceId}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> ReportFailAsync()
        {
            if (!_parentManager.IsRegistered)
            {
                return false;
            }
            //We are connected so lets report to the server
            var response = await _parentManager.Client.GetAsync($"/v1/agent/check/fail/service:{_parentManager.ServiceId}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
