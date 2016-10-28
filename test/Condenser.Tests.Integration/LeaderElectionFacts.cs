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
            regClient1.Config(serviceName: "TestService1", serviceId: "ServiceId1", address: "127.0.0.1", port: 8888);
            regClient1.AddUrls("api/testurl");
            regClient1.AddHealthCheck("/health", 1, 1);
            regClient1.AddLeaderElectionKey("/electionKey/Test");
            await regClient1.RegisterServiceAsync();

            //We should be the leader lets wait on that
            await regClient1.Leader;

            //Next we setup the second service

            var regClient2 = new CondenserDotNet.Client.ServiceRegistrationClient();
            regClient2.Config(serviceName: "TestService1", serviceId: "ServiceId2", address: "127.0.0.1", port: 9999);
            regClient2.AddUrls("api/testurl");
            regClient2.AddLeaderElectionKey("/electionKey/Test");
            regClient2.AddHealthCheck("/health", 1, 1);
            await regClient2.RegisterServiceAsync();

            //By using a clouser this should be true by the time the completion gets called
            var shouldBeLeader = false;
            var wait = new ManualResetEvent(false);

            regClient2.Leader.OnCompleted(() =>
            {
                Assert.True(shouldBeLeader);
                wait.Set();
            });

            //Delay for a a couple of health checks
            await Task.Delay(2000);

            //Close the first client
            server1.Dispose();
            shouldBeLeader = true;

            Assert.True(wait.WaitOne(20000));
        }

        private class MiniServer
        {
            IWebHost _host;

            public MiniServer(int port)
            {
                _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://127.0.0.1:{port}")
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
                app.Run(state => state.Response.WriteAsync("Hello from ASP.NET Core!"));
            }
        }
    }
}
