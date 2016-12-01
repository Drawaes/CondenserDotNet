using System;
using System.Linq;
using System.Text;
using CondenserDotNet.Client.Configuration;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;
using Xunit;

namespace CondenserTests
{
    public class JsonKeyValueParserTests
    {
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