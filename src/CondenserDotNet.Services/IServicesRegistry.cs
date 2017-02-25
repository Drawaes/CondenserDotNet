using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CondenserDotNet.Services
{
    public interface IServicesRegistry
    {
        Task<Dictionary<string, string[]>> GetAvailableServicesAsync();
        Task<ServiceInformation> GetServiceInstanceAsync(string serviceName);
        Task<T> GetFromService<T>(string serviceName, string url);
        Task<TOutput> PostToService<TInput, TOutput>(string serviceName, TInput objectToPost, string url);
    }
}
