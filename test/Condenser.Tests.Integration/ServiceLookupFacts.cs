﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client;
using CondenserDotNet.Client.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Xunit;

namespace Condenser.Tests.Integration
{
    public class ServiceLookupFacts
    {
        [Fact]
        public async Task TestRegisterAndCheckRegistered()
        {
            var key = Guid.NewGuid().ToString();
            var opts = Options.Create(new ServiceManagerConfig() { ServiceName = key, ServiceId = key + "Id1", ServicePort = 2222 });
            var opts2 = Options.Create(new ServiceManagerConfig() { ServiceName = key, ServiceId = key + "Id2", ServicePort = 2222 });
            using (var manager = new ServiceManager(opts))
            using (var manager2 = new ServiceManager(opts2))
            using (var serviceRegistry = new ServiceRegistry())
            {
                var registrationResult = await manager.RegisterServiceAsync();
                Assert.Equal(true, registrationResult);

                var registrationResult2 = await manager2.RegisterServiceAsync();
                Assert.Equal(true, registrationResult2);

                var service = await serviceRegistry.GetServiceInstanceAsync(key);
                Assert.Equal(key, service.Service);
            }
        }

        [Fact]
        public async Task TestThatAnErrorIsReturnedWhenConsulIsNotAvailable()
        {
            using (var serviceRegistry = new ServiceRegistry(() => new HttpClient() { BaseAddress = new Uri( "http://localhost:7000" )}))
            {
                await Assert.ThrowsAsync<NoServiceInstanceFoundException>(async () => await serviceRegistry.GetServiceInstanceAsync("TestService"));
            }
        }

        [Fact]
        public async Task TestRegisterAndCheckUpdates()
        {
            var serviceName = Guid.NewGuid().ToString();
            var opts = Options.Create(new ServiceManagerConfig() { ServiceName = serviceName, ServiceId = serviceName, ServicePort = 2222 });
            using (var manager = new ServiceManager(opts))
            using (var serviceRegistry = new ServiceRegistry())
            {
                Assert.Null(await serviceRegistry.GetServiceInstanceAsync(serviceName));
                await manager.RegisterServiceAsync();
                //Give it 500ms to update with the new service
                await Task.Delay(500);
                Assert.NotNull(await serviceRegistry.GetServiceInstanceAsync(serviceName));
            }
        }
    }
}