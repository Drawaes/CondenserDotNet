using CondenserDotNet.Client;
using CondenserTests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CondenserTests
{
    public class HealthConfigurationFacts
    {
        [Theory]
        [InlineData("http://someotheraddress/health", true, "http://someotheraddress/health")]
        [InlineData("/health", true, "https://localhost:8000/health")]
        [InlineData("health", false, "https://localhost:8000/health")]
        public void ShouldBuildHttpsExpectedUrl(string url, bool ignoreForCheck, string expectedUrl)
        {
            var id = "myservice";
            var interval = 50;

            var manager = new FakeServiceManager
            {
                ServicePort = 8000,
                ServiceAddress = "localhost",
                ServiceId = id                
            };

            manager.AddHttpHealthCheck(url, interval);
            manager.UseHttps(ignoreForCheck);

            var check = manager.HealthConfig.Build(manager);

            Assert.Equal(expectedUrl, check.HTTP);
            Assert.Equal(ignoreForCheck, check.tls_skip_verify);                
        }

        [Theory]
        [InlineData("http://someotheraddress/health", "http://someotheraddress/health")]
        [InlineData("/health", "http://localhost:8000/health")]
        [InlineData("health", "http://localhost:8000/health")]
        public void ShouldBuildExpectedUrl(string url, string expectedUrl)
        {
            var id = "myservice";
            var interval = 50;

            var manager = new FakeServiceManager
            {
                ServicePort = 8000,
                ServiceAddress = "localhost",
                ServiceId = id
            };

            manager.AddHttpHealthCheck(url, interval);

            var check = manager.HealthConfig.Build(manager);

            Assert.Equal(expectedUrl, check.HTTP);
            Assert.Equal($"{interval}s", check.Interval);
            Assert.Equal($"{id}:HttpCheck", check.Name);
        }

        [Fact]
        public void ShouldBuildNoHealthCheckWhenUrlNotSet()
        {
            var id = "myservice";

            var manager = new FakeServiceManager
            {
                ServicePort = 8000,
                ServiceAddress = "localhost",
                ServiceId = id
            };

            var check = manager.HealthConfig.Build(manager);
            Assert.Null(check);
        }
    }
}
