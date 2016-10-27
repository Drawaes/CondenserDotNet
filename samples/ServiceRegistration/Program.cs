using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Microsoft.AspNetCore.Hosting;

namespace ServiceRegistration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceEndpointClient serviceClient = new ServiceEndpointClient();
            serviceClient.LoadAvailableDataCenters().Wait();
            serviceClient.LoadAvailableServices().Wait();

            var regClient = new ServiceRegistrationClient();
            regClient
                .Config(serviceName: "timsService", port: 7777, address: "localhost")
                .AddSupportedVersions(new Version(1, 0, 0))
                .AddHealthCheck("Health", 10, 20)
                .AddLeaderElectionKey("/test/leaderElection");
            regClient.RegisterServiceAsync().Wait();

            regClient.Leader.WaitAsync().OnCompleted(() => Console.WriteLine("I am leader!"));

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:7777")
                .UseStartup<Startup>()
                .Build();

            host.Run();
                       
        }
    }
}
