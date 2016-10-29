using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;

namespace CondenserDotNet.Client
{
    public class Leader : ClientBase
    {
        private Guid _sessionKey = Guid.NewGuid();
        private string _sessionCreateString;
        private static readonly string _sessionCreateQueryString = "/v1/session/create";
        private string _keyname;
        private string _serviceId;

        public Leader(string keyname, string serviceId)
        {
            _serviceId = serviceId;
            _keyname = keyname;

            var sessionCreate = new SessionCreate()
            {
                Behavior = "release",
                Checks = new string[] { "serfHealth", $"service:{serviceId}" },
                LockDelay = "2s",
                Name = $"{_serviceId}:LeaderElection",
            };
            _sessionCreateString = JsonConvert.SerializeObject(sessionCreate);

            StartSession();
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
                    break;
                }
                //Failed do something for the retry
                await Task.Delay(5000);
            }

            var sessionResponse = JsonConvert.DeserializeObject<SessionCreateResponse>(await sessionCreateReturn.Content.ReadAsStringAsync());

            //Now try to get the lock

            var putSession = new StringContent(_serviceId, System.Text.Encoding.UTF8);
            var lockResponse = await _httpClient.PutAsync($"/v1/kv/{_keyname}?acquire={sessionResponse.Id}", putSession);
            var responseValue = await lockResponse.Content.ReadAsStringAsync();
        }

        //public Task
    }
}
