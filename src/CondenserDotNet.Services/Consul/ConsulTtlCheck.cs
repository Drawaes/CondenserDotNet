using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace CondenserDotNet.Services.Consul
{
    public class ConsulTtlCheck
    {
        private int _timeToLiveSeconds;
        private HealthCheck _healthCheck;
        private ConsulServiceRegistration _registration;
        private HttpClient _client;

        public ConsulTtlCheck(ConsulServiceRegistration registration, int timeToLiveSeconds, HttpClient client)
        {
            _registration = registration;
            _client = client;
            _healthCheck = new HealthCheck()
            {
                TTL = $"{timeToLiveSeconds}s"
            };
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public HealthCheck HealthCheck => _healthCheck;

        public async Task<bool> ReportPassingAsync()
        {
            if (!_registration.IsRegistered)
            {
                return false;
            }
            //We are connected so lets report to the server
            var response = await _client.GetAsync($"/v1/agent/check/pass/service:{_registration.ServiceId}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> ReportWarningAsync()
        {
            if (!_registration.IsRegistered)
            {
                return false;
            }
            //We are connected so lets report to the server
            var response = await _client.GetAsync($"/v1/agent/check/warn/service:{_registration.ServiceId}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> ReportFailAsync()
        {
            if (!_registration.IsRegistered)
            {
                return false;
            }
            //We are connected so lets report to the server
            var response = await _client.GetAsync($"/v1/agent/check/fail/service:{_registration.ServiceId}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
