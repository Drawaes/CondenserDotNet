using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondenserDotNet.Service
{
    public interface IServiceRegistry
    {
        Task<IEnumerable<string>> GetAvailableServicesAsync();
        Task<Dictionary<string, string[]>> GetAvailableServicesWithTagsAsync();
        Task<DataContracts.InformationService> GetServiceInstanceAsync(string serviceName);
    }
}