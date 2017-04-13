using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondenserDotNet.Configuration.Consul
{
    public interface IConfigSource
    {
        object CreateWatchState();
        string FormValidKey(string keyPath);
        Task<(bool success, Dictionary<string, string> dictionary)> GetKeysAsync(string keyPath);
        Task<(bool success, Dictionary<string, string> update)> TryWatchKeysAsync(string keyPath, object state);

        Task<bool> TrySetKeyAsync(string keyPath, string value);
    }
}