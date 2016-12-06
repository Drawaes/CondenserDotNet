using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ConfigFacts
    {
        const string _value1 = "testValue1";
        const string _value2 = "testValue2";
        const string _value3 = "testValue3";
        const string _value4 = "testValue4";

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
                manager.Config.AddWatchOnSingleKey("test1", keyValue => e.Set());

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
                manager.Config.AddWatchOnSingleKey("test1", keyValue => e.Set());

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

        [Fact]
        public async Task CanLoadUsingConsulConfigurationProvider()
        {
            var keyname = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                await manager.Config.SetKeyAsync($"org/{keyname}/test1", _value1);
                await manager.Config.SetKeyAsync($"org/{keyname}/test2", _value2);
                await manager.Config.SetKeyAsync($"org/{keyname}/Nested/test3", _value3);
                await manager.Config.SetKeyAsync($"org/{keyname}/Nested/test4", _value4);

                await manager.Config.AddStaticKeyPathAsync($"org");

                var config = new ConfigurationBuilder()
                    .AddConsul(manager.Config)
                    .Build();
                
                var simpleSettings = new SimpleSettings();
                var configSection = config.GetSection(keyname);
                configSection.Bind(simpleSettings);

                Assert.Equal(_value1, simpleSettings.Test1);
                Assert.Equal(_value2, simpleSettings.Test2);
                Assert.Equal(_value3, simpleSettings.Nested.Test3);
                Assert.Equal(_value4, simpleSettings.Nested.Test4);
            }
        }

        [Fact]
        public async Task CanLoadUsingConsulJsonConfigurationProvider()
        {
            var keyname = Guid.NewGuid().ToString();
            using (var manager = new ServiceManager("TestService"))
            {
                var settings = new SimpleSettings
                {
                    Test1 = _value1,
                    Test2 = _value2,
                    Nested = new SimpleNestedSettings
                    {
                        Test3 = _value3,
                        Test4 = _value4
                    }
                };

                await manager.Config.SetKeyJsonAsync($"org/{keyname}/Settings", settings);

                var config = new ConfigurationBuilder()
                    .AddJsonConsul(manager.Config)
                    .Build();

                //TODO: At the moment require this line after builder as it sets parser.  Probably need to rethink this.
                await manager.Config.AddStaticKeyPathAsync($"org/{keyname}");

                var simpleSettings = new SimpleSettings();
                var configSection = config.GetSection("Settings");
                configSection.Bind(simpleSettings);

                Assert.Equal(_value1, simpleSettings.Test1);
                Assert.Equal(_value2, simpleSettings.Test2);
                Assert.Equal(_value3, simpleSettings.Nested.Test3);
                Assert.Equal(_value4, simpleSettings.Nested.Test4);
            }
        }

        private class SimpleSettings
        {
            public string Test1 { get; set; }
            public string Test2 { get; set; }

            public SimpleNestedSettings Nested { get; set; }
        }

        private class SimpleNestedSettings
        {
            public string Test3 { get; set; }
            public string Test4 { get; set; }
        }
        
    }
}
