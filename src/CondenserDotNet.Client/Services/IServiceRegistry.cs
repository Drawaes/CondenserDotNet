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
    }
}