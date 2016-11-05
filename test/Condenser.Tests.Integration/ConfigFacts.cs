using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ConfigFacts
    {
        const string _value1 = "testValue1";
        const string _value2 = "testValue2";

        [Fact]
        public async Task TestRegister()
        {
            var keyname = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                await manager.Config.SetKeyAsync($"org/{keyname}/test1", _value1);
                await manager.Config.SetKeyAsync($"org/{keyname}/test2", _value2);

                var result = await manager.Config.AddStaticKeyPathAsync($"org/{keyname}");

                var firstValue = manager.Config["test1"];
                var secondValue = manager.Config["test2"];

                Assert.Equal(_value1, firstValue);
                Assert.Equal(_value2, secondValue);
            }
        }

        [Fact]
        public async Task DontPickUpChangesFact()
        {
            var keyname = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                await manager.Config.SetKeyAsync($"org/{keyname}/test1", _value1);

                var result = await manager.Config.AddStaticKeyPathAsync($"org/{keyname}");
                await manager.Config.SetKeyAsync($"org/{keyname}/test1", _value2);

                await Task.Delay(500); //give some time to sync
                var firstValue = manager.Config["test1"];
                Assert.Equal(_value1, firstValue);
            }
        }

        [Fact]
        public async Task PickUpChangesFact()
        {
            string keyid = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                await manager.Config.SetKeyAsync($"org/{keyid}/test2", _value1);
                await manager.Config.AddUpdatingPathAsync($"org/{keyid}/");

                await Task.Delay(500);

                Assert.Equal(_value1, manager.Config["test2"]);
                await manager.Config.SetKeyAsync($"org/{keyid}/test2", _value2);

                await Task.Delay(500); //give some time to sync
                Assert.Equal(_value2, manager.Config["test2"]);
            }
        }

        [Fact]
        public async Task GetCallbackForSpecificKey()
        {
            Console.WriteLine(nameof(GetCallbackForSpecificKey));
            string keyid = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                var e = new ManualResetEvent(false);
                manager.Config.AddWatchOnSingleKey("test1", () => e.Set());

                await manager.Config.SetKeyAsync($"org/{keyid}/test1", _value1);
                await manager.Config.AddUpdatingPathAsync($"org/{keyid}/");

                await Task.Delay(1000);

                await manager.Config.SetKeyAsync($"org/{keyid}/test1", _value2);

                //Wait for a max of 1 second for the change to notify us
                Assert.True(e.WaitOne(2000));
            }
        }

        [Fact]
        public async Task GetCallbackForKeyThatIsAdded()
        {
            string keyid = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                var e = new ManualResetEvent(false);
                manager.Config.AddWatchOnSingleKey("test1", () => e.Set());

                await manager.Config.AddUpdatingPathAsync($"org/{keyid}/");

                //give it time to register
                await Task.Delay(1000);

                await manager.Config.SetKeyAsync($"org/{keyid}/test1", _value2);

                //Wait for a max of 1 second for the change to notify us
                Assert.True(e.WaitOne(2000));
            }
        }

        [Fact]
        public async Task GetCallbackForAnyKey()
        {
            Console.WriteLine(nameof(GetCallbackForAnyKey));
            string keyid = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                await manager.Config.SetKeyAsync($"org/{keyid}/test1", _value1);

                await manager.Config.AddUpdatingPathAsync($"org/{keyid}");

                var e = new ManualResetEvent(false);
                manager.Config.AddWatchOnEntireConfig(() => e.Set());

                //give the registration time to complete registration
                await Task.Delay(1000);

                await manager.Config.SetKeyAsync($"org/{keyid}/test1", _value2);

                //Wait for a max of 1 second for the change to notify us
                Assert.True(e.WaitOne(2000));
            }
        }
    }
}
