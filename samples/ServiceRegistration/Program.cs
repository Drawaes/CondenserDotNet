﻿using System;
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
            var serviceManager = new ServiceManager("TestService");

            serviceManager.AddHttpHealthCheck("/Health",10)
                .AddApiUrl("/testsample/test3/test2")
                .AddApiUrl("/testSample/test3/test1")
                .RegisterServiceAsync().Wait();
            
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{serviceManager.ServicePort}")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
