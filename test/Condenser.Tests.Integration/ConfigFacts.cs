using System;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Configuration;
using CondenserDotNet.Configuration.Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
            using (var configRegistry = new ConsulRegistry())
            {
                await configRegistry.SetKeyAsync($"org/{keyname}/test1", _value1);
                await configRegistry.SetKeyAsync($"org/{keyname}/test2", _value2);

                var result = await configRegistry.AddStaticKeyPathAsync($"org/{keyname}");
                Assert.True(result);

                var firstValue = configRegistry["test1"];
                var secondValue = configRegistry["test2"];

                Assert.Equal(_value1, firstValue);
                Assert.Equal(_value2, secondValue);
            }
        }

        [Fact]
        public async Task TestKeyHandlesFrontSlash()
        {
            using (var registry = CondenserConfigBuilder.FromConsul().Build())
            {
                var keyname = Guid.NewGuid().ToString();
                await registry.SetKeyAsync($"org/{keyname}/test1", _value1);

                var result = await registry.AddStaticKeyPathAsync($"/org/{keyname}");
                Assert.True(result);

                var firstValue = registry["test1"];

                Assert.Equal(_value1, firstValue);
            }
        }

        [Fact]
        public async Task DontPickUpChangesFact()
        {
            var keyname = Guid.NewGuid().ToString();
            using (var configRegistry = new ConsulRegistry())
            {
                await configRegistry.SetKeyAsync($"org/{keyname}/test1", _value1);

                var result = await configRegistry.AddStaticKeyPathAsync($"org/{keyname}");
                await configRegistry.SetKeyAsync($"org/{keyname}/test1", _value2);

                await Task.Delay(500); //give some time to sync
                var firstValue = configRegistry["test1"];
                Assert.Equal(_value1, firstValue);
            }
        }

        [Fact]
        public async Task PickUpChangesFact()
        {
            var keyid = Guid.NewGuid().ToString();
            using (var configRegistry = new ConsulRegistry())
            {
                await configRegistry.SetKeyAsync($"org/{keyid}/test2", _value1);
                await configRegistry.AddUpdatingPathAsync($"org/{keyid}/");

                await Task.Delay(500);

                Assert.Equal(_value1, configRegistry["test2"]);
                await configRegistry.SetKeyAsync($"org/{keyid}/test2", _value2);

                await Task.Delay(500); //give some time to sync
                Assert.Equal(_value2, configRegistry["test2"]);
            }
        }

        [Fact]
        public async Task GetCallbackForSpecificKey()
        {
            Console.WriteLine(nameof(GetCallbackForSpecificKey));
            var keyid = Guid.NewGuid().ToString();
            using (var configRegistry = new ConsulRegistry())
            {
                var e = new ManualResetEvent(false);
                configRegistry.AddWatchOnSingleKey("test1", keyValue => e.Set());

                await configRegistry.SetKeyAsync($"org/{keyid}/test1", _value1);
                await configRegistry.AddUpdatingPathAsync($"org/{keyid}/");

                await Task.Delay(1000);

                await configRegistry.SetKeyAsync($"org/{keyid}/test1", _value2);

                //Wait for a max of 1 second for the change to notify us
                Assert.True(e.WaitOne(2000));
            }
        }

        [Fact]
        public async Task GetCallbackForKeyThatIsAdded()
        {
            var keyid = Guid.NewGuid().ToString();
            using (var configRegistry = new ConsulRegistry())
            {
                var e = new ManualResetEvent(false);
                configRegistry.AddWatchOnSingleKey("test1", keyValue => e.Set());

                await configRegistry.AddUpdatingPathAsync($"org/{keyid}/");

                //give it time to register
                await Task.Delay(1000);

                await configRegistry.SetKeyAsync($"org/{keyid}/test1", _value2);

                //Wait for a max of 1 second for the change to notify us
                Assert.True(e.WaitOne(2000));
            }
        }

        [Fact]
        public async Task GetCallbackForAnyKey()
        {
            Console.WriteLine(nameof(GetCallbackForAnyKey));
            var keyid = Guid.NewGuid().ToString();
            using (var configRegistry = new ConsulRegistry())
            {
                await configRegistry.SetKeyAsync($"org/{keyid}/test1", _value1);

                await configRegistry.AddUpdatingPathAsync($"org/{keyid}");

                var e = new ManualResetEvent(false);
                configRegistry.AddWatchOnEntireConfig(() => e.Set());

                //give the registration time to complete registration
                await Task.Delay(1000);

                await configRegistry.SetKeyAsync($"org/{keyid}/test1", _value2);

                //Wait for a max of 1 second for the change to notify us
                Assert.True(e.WaitOne(2000));
            }
        }

        [Fact]
        public async Task CanLoadUsingConsulConfigurationProvider()
        {
            var keyname = Guid.NewGuid().ToString();
            using (var configRegistry = new ConsulRegistry())
            {
                await configRegistry.SetKeyAsync($"org/{keyname}/test1", _value1);
                await configRegistry.SetKeyAsync($"org/{keyname}/test2", _value2);
                await configRegistry.SetKeyAsync($"org/{keyname}/Nested/test3", _value3);
                await configRegistry.SetKeyAsync($"org/{keyname}/Nested/test4", _value4);

                await configRegistry.AddStaticKeyPathAsync($"org");

                var config = new ConfigurationBuilder()
                    .AddConfigurationRegistry(configRegistry)
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
            using (var configRegistry = new ConsulRegistry(Options.Create<ConsulRegistryConfig>(new ConsulRegistryConfig() { KeyParser = new JsonKeyValueParser() })))
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

                await configRegistry.SetKeyJsonAsync($"org/{keyname}/Settings", settings);

                var config = new ConfigurationBuilder()
                    .AddConfigurationRegistry(configRegistry)
                    .Build();

                //TODO: At the moment require this line after builder as it sets parser.  Probably need to rethink this.
                await configRegistry.AddStaticKeyPathAsync($"org/{keyname}");

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
