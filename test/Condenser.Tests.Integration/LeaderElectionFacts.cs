using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class LeaderElectionFacts
    {
        [Fact]
        public async void TestLeaderLeaving()
        {
            var server1 = new MiniServer(8888);
            var server2 = new MiniServer(9999);

            var regClient1 = new CondenserDotNet.Client.ServiceRegistrationClient();
            regClient1.Config(serviceName: "TestService1", serviceId: "ServiceId1", address: "localhost", port: 8888);
            regClient1.AddUrls("api/testurl");
            regClient1.AddHealthCheck("/health",2,2);
            regClient1.AddLeaderElectionKey("/electionKey/Test");
            await regClient1.RegisterServiceAsync();

            //We should be the leader lets wait on that
            await regClient1.Leader;

            //Next we setup the second service

            var regClient2 = new CondenserDotNet.Client.ServiceRegistrationClient();
            regClient2.Config(serviceName: "TestService1", serviceId: "ServiceId2", address: "localhost", port: 9999);
            regClient2.AddUrls("api/testurl");
            regClient2.AddLeaderElectionKey("/electionKey/Test");
            regClient2.AddHealthCheck("/health", 2, 2);
            await regClient2.RegisterServiceAsync();

            //Because this is a closure it will be false if the awaiter finishes before the leader election does
            bool shouldBeLeader = false;
            var wait = new ManualResetEvent(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                await regClient2.Leader;
                Assert.True(shouldBeLeader);
                wait.Set();
             });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            //Delay for a a couple of health checks
            await Task.Delay(5000);

            //Now we can release the leader
            shouldBeLeader = true;

            ////Close the first client
            server1.Dispose();

            //Now we wait for the second to become leader for 5 seconds or blow up because it should have happened
            Assert.True(wait.WaitOne(15000));
        }

        private class MiniServer
        {
            IWebHost _host;

            public MiniServer(int port)
            {
                _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{port}")
                .UseStartup<DummyHealthServer>()
                .Build();

                _host.Start();
            }

            public void Dispose()
            {
                _host.Dispose();
            }
        }

        public class DummyHealthServer
        {
            public void Configure(IApplicationBuilder app)
            {
                app.Run( state => state.Response.WriteAsync("Hello from ASP.NET Core!"));
            }
        }
    }
}
