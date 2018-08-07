using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondenserDotNet.Configuration.Consul
{
    public interface IConfigSource
    {
        object CreateWatchState();
        string FormValidKey(string keyPath);
        Task<KeyOperationResult> GetKeysAsync(string keyPath);
        Task<KeyOperationResult> TryWatchKeysAsync(string keyPath, object state);
        Task<(bool found, string value)> GetKeyAsync(string keyPath);
        Task<bool> TrySetKeyAsync(string keyPath, string value);
    }
}
