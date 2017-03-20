using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CondenserDotNet.Configuration.Consul;
using CondenserTests.Fakes;
using Newtonsoft.Json;
using Xunit;

namespace CondenserTests
{
    public class JsonKeyValueParserTests
    {
        [Fact]
        public void CanParseList()
        {
            var fakeConfigs = new List<FakeConfig>
            {
                new FakeConfig
                {
                    Setting1 = "one",
                    Setting2 = "two"
                },
                new FakeConfig
                {
                    Setting1 = "three",
                    Setting2 = "four"
                }
            };
            var key = "my/config/section/objectlist";
            var json = JsonConvert.SerializeObject(fakeConfigs);
            var parser = new JsonKeyValueParser();
            var keyValue = new KeyValue
            {
                Key = key,
                Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            };

            var keys = parser.Parse(keyValue)
                .ToArray();

            Assert.Equal(4, keys.Length);
            Assert.Equal("my/config/section/objectlist:0:Setting1", keys[0].Key);
            Assert.Equal("my/config/section/objectlist:0:Setting2", keys[1].Key);
            Assert.Equal("my/config/section/objectlist:1:Setting1", keys[2].Key);
            Assert.Equal("my/config/section/objectlist:1:Setting2", keys[3].Key);
        }

        [Fact]
        public void CanParseJsonObjectToRequireOptionsFormat()
        {
            var dto = JsonConvert.SerializeObject(new
            {
                Port = 1234,
                Server = "localhost"
            });

            var keyValue = new KeyValue
            {
                Key = "smtp",
                Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto))
            };

            var data = new JsonKeyValueParser()
                .Parse(keyValue)
                .ToArray();

            Assert.Equal(2, data.Length);
            Assert.Equal("smtp:Port", data[0].Key);
            Assert.Equal("1234", data[0].Value);
            Assert.Equal("smtp:Server", data[1].Key);
            Assert.Equal("localhost", data[1].Value);
        }
    }
}