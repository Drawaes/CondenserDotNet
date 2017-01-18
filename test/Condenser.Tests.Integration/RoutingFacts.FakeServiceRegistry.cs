using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Service;
using CondenserDotNet.Service.DataContracts;

namespace Condenser.Tests.Integration
{
    public class FakeServiceRegistry : IServiceRegistry
    {
        private InformationService _informationService;

        public Task<IEnumerable<string>> GetAvailableServicesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string[]>> GetAvailableServicesWithTagsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<InformationService> GetServiceInstanceAsync(string serviceName)
        {
            return Task.FromResult(_informationService);
        }

        public void SetServiceInstance(InformationService informationService)
        {
            _informationService = informationService;
        }
    }
}