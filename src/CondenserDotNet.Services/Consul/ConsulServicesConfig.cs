using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CondenserDotNet.Services.Consul
{
    public class ConsulServicesConfig
    {
        public ConsulServicesConfig()
        {
            HttpClientFactory = () => new HttpClient();
            AgentAddress = "localhost";
            AgentPort = 8500;
        }

        public Func<HttpClient> HttpClientFactory { get;set;}
        public string AgentAddress { get;set;}
        public int AgentPort { get;set;}
    }
}
