using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class LeadershipFacts
    {
        [Fact]
        public async Task TestGetLeadership()
        {
            Console.WriteLine(nameof(TestGetLeadership));
            using (var manager = new ServiceManager("TestService2"))
            {
                manager.AddTtlHealthCheck(10);
                var registerResult = await manager.RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();

                var watcher = manager.Leaders.GetLeaderWatcher("leadershipTests/leadershipTestBasic");
                await watcher.GetLeadershipAsync();
                var result = await watcher.GetCurrentLeaderAsync();
                Assert.Equal(manager.ServiceId, result.ID);
            }
        }

        [Fact(Skip = "Broken")]
        public async Task TestLeadershipFailOver()
        {
            Console.WriteLine(nameof(TestLeadershipFailOver));
            using (var manager = new ServiceManager("LeaderService1"))
            using (var manager2 = new ServiceManager("LeaderService1", "LeaderId1"))
            {
                await manager.AddTtlHealthCheck(100).RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();

                await manager2.AddTtlHealthCheck(100).RegisterServiceAsync();
                await manager2.TtlCheck.ReportPassingAsync();

                var watcher1 = manager.Leaders.GetLeaderWatcher("leadershipTests/leadershipTestFailOver");
                await watcher1.GetLeadershipAsync();
                bool shouldNotBeLeader = true;
                var resetEvent = new ManualResetEvent(false);

                //Now that 1 is the leader lets try to join 2 into the party
                var watcher2 = manager2.Leaders.GetLeaderWatcher("leadershipTests/leadershipTestFailOver");
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
                Assert.True(resetEvent.WaitOne(2000));

                //Now check that service 2 is the leader
                var leader = await watcher2.GetCurrentLeaderAsync();
                Assert.Equal(manager2.ServiceId, leader.ID);
            }
        }
    }
}
