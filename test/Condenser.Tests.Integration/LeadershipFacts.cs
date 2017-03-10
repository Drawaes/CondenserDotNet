using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Xunit;
using CondenserDotNet.Client.Leadership;
using Microsoft.Extensions.Options;

namespace Condenser.Tests.Integration
{
    public class LeadershipFacts
    {
        [Fact]
        public async Task TestGetLeadership()
        {
            var key = Guid.NewGuid().ToString();
            var leadershipKey = $"leadershipTests/{key}";
            Console.WriteLine(nameof(TestGetLeadership));
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222, ServiceName = key });
            using (var manager = new ServiceManager(opts))
            {
                manager.AddTtlHealthCheck(10);
                var registerResult = await manager.RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();
                var leaderRegistry = new LeaderRegistry(manager);
                var watcher = leaderRegistry.GetLeaderWatcher(leadershipKey);
                await watcher.GetLeadershipAsync();
                var result = await watcher.GetCurrentLeaderAsync();
                Assert.Equal(manager.ServiceId, result.ID);
            }
        }

        [Fact]
        public async Task TestLeadershipFailOver()
        {
            var key = Guid.NewGuid().ToString();
            var leadershipKey = $"leadershipTests/{key}";
            Console.WriteLine(nameof(TestLeadershipFailOver));
            var opts = Options.Create(new ServiceManagerConfig() { ServicePort = 2222, ServiceName = key });
            var opts2 = Options.Create(new ServiceManagerConfig() { ServicePort = 2222, ServiceName = $"{key}LeaderId1" });
            using (var manager = new ServiceManager(opts))
            using (var manager2 = new ServiceManager(opts2))
            {
                await manager.AddTtlHealthCheck(100).RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();

                await manager2.AddTtlHealthCheck(100).RegisterServiceAsync();
                await manager2.TtlCheck.ReportPassingAsync();

                var watcher1 = (new LeaderRegistry(manager)).GetLeaderWatcher(leadershipKey);
                await watcher1.GetLeadershipAsync();
                bool shouldNotBeLeader = true;
                var resetEvent = new ManualResetEvent(false);

                //Now that 1 is the leader lets try to join 2 into the party
                var watcher2 = (new LeaderRegistry(manager2)).GetLeaderWatcher(leadershipKey);
                var result = watcher2.GetLeadershipAsync().ContinueWith(t =>
                {
                    Assert.False(shouldNotBeLeader);
                    resetEvent.Set();
                });

                //Wait to make sure we didn't fail over straight away
                await Task.Delay(1000);

                //Now report that the first service is failing
                shouldNotBeLeader = false;
                await manager.TtlCheck.ReportFailAsync();

                //Now we wait, the leadership should fall over
                Assert.True(resetEvent.WaitOne(5000));

                //Now check that service 2 is the leader
                var leader = await watcher2.GetCurrentLeaderAsync();
                Assert.Equal(manager2.ServiceId, leader.ID);
            }
        }
    }
}
