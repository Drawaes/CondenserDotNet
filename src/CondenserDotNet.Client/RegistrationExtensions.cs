using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Client.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    public static class RegistrationExtensions
    {
        public static ServiceManager AddApiUrl(this ServiceManager serviceManager, string urlToAdd)
        {
            serviceManager.SupportedUrls.Add(urlToAdd);
            return serviceManager;
        }

        public static ServiceManager AddHttpHealthCheck(this ServiceManager serviceManager, string url, int intervalInSeconds)
        {
            HealthCheck check = new HealthCheck()
            {
                HTTP = $"{serviceManager.ServiceAddress}:{serviceManager.ServicePort}{url}",
                Interval = $"{intervalInSeconds}s",
                Name = $"{serviceManager.ServiceId}:HttpCheck"
            };
            if(!check.HTTP.StartsWith("HTTP",StringComparison.OrdinalIgnoreCase))
            {
                check.HTTP = "http://" + check.HTTP;
            }
            serviceManager.HttpCheck = check;
            return serviceManager;
        }

        public static ServiceManager AddTtlHealthCheck(this ServiceManager serviceManager, int timetoLiveInSeconds)
        {
            serviceManager.TtlCheck = new TtlCheck(serviceManager, timetoLiveInSeconds);
            return serviceManager;
        }

        public static async Task<bool> RegisterServiceAsync(this ServiceManager serviceManager)
        {
            Service s = new Service()
            {
                Address = serviceManager.ServiceAddress,
                EnableTagOverride = false,
                ID = serviceManager.ServiceId,
                Name = serviceManager.ServiceName,
                Port = serviceManager.ServicePort,
                Checks = new List<HealthCheck>(),
                Tags = new List<string>(serviceManager.SupportedUrls.Select(u => $"urlprefix-{u}"))
            };
            if (serviceManager.HttpCheck != null)
            {
                s.Checks.Add(serviceManager.HttpCheck);
            }
            if (serviceManager.TtlCheck != null)
            {
                s.Checks.Add(serviceManager.TtlCheck.HealthCheck);
            }
            if (s.Checks.Count > 1)
            {
                for (int i = 0; i < s.Checks.Count; i++)
                {
                    s.Checks[i].Name = $"service:{s.ID}:{i + 1}";
                }
            }
            else if (s.Checks.Count == 1)
            {
                s.Checks[0].Name = $"service:{s.ID}";
            }
            var content = HttpUtils.GetStringContent(s);
            var response = await serviceManager.Client.PutAsync("/v1/agent/service/register", content);
            if (response.IsSuccessStatusCode)
            {
                serviceManager.RegisteredService = s;
                return true;
            }
            serviceManager.RegisteredService = null;
            return false;
        }
    }
}

