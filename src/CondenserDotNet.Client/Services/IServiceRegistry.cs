using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;

namespace CondenserDotNet.Client.Services
{
    public interface IServiceRegistry
    {
        Task<IEnumerable<string>> GetAvailableServicesAsync();
        Task<Dictionary<string, string[]>> GetAvailableServicesWithTagsAsync();
        Task<InformationService> GetServiceInstanceAsync(string serviceName);
        Task<InformationService> GetNearestServiceInstanceAsync(string serviceName);
        ServiceBasedHttpHandler GetHttpHandler();
        ServiceBasedHttpHandler GetHttpHandler(int maxConnectionsPerServer);
        WatcherState GetServiceCurrentState(string serviceName);
        void SetServiceListCallback(string serviceName, Action<List<InformationServiceSet>> callback);
    }
}
