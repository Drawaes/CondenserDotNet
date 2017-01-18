using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Service;
using CondenserDotNet.Service.DataContracts;

namespace CondenserTests.Fakes
{
    public class FakeServiceRegistry : IServiceRegistry
    {
        private readonly List<InformationService> _services = new List<InformationService>();

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
            var service = _services.SingleOrDefault(x => x.Service == serviceName);
            return Task.FromResult(service);
        }

        public void AddServiceInstance(InformationService informationService)
        {
            _services.Add(informationService);
        }

        public void AddServiceInstance(string address, int port)
        {
            AddServiceInstance(new InformationService
            {
                Port = port,
                Address = address
            });
        }
    }
}