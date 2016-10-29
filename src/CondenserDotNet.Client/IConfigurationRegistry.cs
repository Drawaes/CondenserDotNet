using System;
using System.Threading.Tasks;

namespace CondenserDotNet.Client
{
    public interface IConfigurationRegistry
    {
        string this[string key] { get; }

        Task<bool> AddStaticKeyPathAsync(string keyPath);
        Task<bool> AddUpdatingPathAsync(string keyPath);
        void AddWatchOnEntireConfig(Action callback);
        void AddWatchOnSingleKey(string keyToWatch, Action callback);
        Task<bool> SetKeyAsync(string keyPath, string value);
        bool TryGetValue(string key, out string value);
    }
}