using System;
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
        private readonly string leadershipKey;
        private readonly Guid key;

        public LeadershipFacts()
        {
            key = Guid.NewGuid();
            leadershipKey = $"leadershipTests/{key}";
        }

        private ServiceManager GetConfig(string serviceId) =>
            new ServiceManager(
                Options.Create(
                    new ServiceManagerConfig()
                    {
                        ServicePort = 2222,
                        ServiceName = $"{key}{serviceId}"
                    }));

        [Fact]
        public async Task FailToGetLeadershipWhenNotRegistered()
        {
            using (var manager = GetConfig("Service1"))
            {
                var leadership = new LeaderRegistry(manager);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await leadership.GetLeaderWatcherAsync("TestKey"));
            }
        }

        [Fact]
        public async Task TestLeadershipIsBlocking()
        {
            using (var manager = GetConfig("Service1"))
            using (var manager2 = GetConfig("Service2"))
            {
                await manager.AddTtlHealthCheck(1000).RegisterServiceAsync();
                await manager2.AddTtlHealthCheck(1000).RegisterServiceAsync();
                await manager.TtlCheck.ReportPassingAsync();
                await manager2.TtlCheck.ReportPassingAsync();
                var leader1 = new LeaderRegistry(manager);
                var leader2 = new LeaderRegistry(manager2);

                var watcher = await leader1.GetLeaderWatcherAsync(leadershipKey);
                var watcher2 = await leader2.GetLeaderWatcherAsync(leadershipKey);
                var counter = 0;

                watcher.SetLeaderCallback(info => Interlocked.Increment(ref counter));
                watcher2.SetLeaderCallback(info => Interlocked.Increment(ref counter));

                var ignore = watcher.GetLeadershipAsync();
                var ignore2 = watcher.GetLeadershipAsync();

                await Task.Delay(1000);
                Assert.InRange(counter, 0, 5);
            }
        }

        [Fact]
        public async Task TestGetLeadership()
        {

            using (var manager = GetConfig("Service1"))
            {
                manager.AddTtlHealthCheck(10);
                var registerResult = await manager.RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();
                var leaderRegistry = new LeaderRegistry(manager);
                var watcher = await leaderRegistry.GetLeaderWatcherAsync(leadershipKey);
                await watcher.GetLeadershipAsync();
                var result = await watcher.GetCurrentLeaderAsync();
                Assert.Equal(manager.ServiceId, result.ID);
            }
        }

        [Fact]
        public async Task TestLeadershipFailOver()
        {
            using (var manager = GetConfig("Service1"))
            using (var manager2 = GetConfig("Service2"))
            {
                await manager.AddTtlHealthCheck(100).RegisterServiceAsync();
                var ttlResult = await manager.TtlCheck.ReportPassingAsync();

                await manager2.AddTtlHealthCheck(100).RegisterServiceAsync();
                await manager2.TtlCheck.ReportPassingAsync();

                var watcher1 = await (new LeaderRegistry(manager)).GetLeaderWatcherAsync(leadershipKey);
                await watcher1.GetLeadershipAsync();
                var shouldNotBeLeader = true;
                var resetEvent = new ManualResetEvent(false);

                //Now that 1 is the leader lets try to join 2 into the party
                var watcher2 = await (new LeaderRegistry(manager2)).GetLeaderWatcherAsync(leadershipKey);
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
