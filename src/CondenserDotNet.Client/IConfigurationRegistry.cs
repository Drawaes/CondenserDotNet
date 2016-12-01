using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CondenserDotNet.Client.Configuration;

namespace CondenserDotNet.Client
{
    public interface IConfigurationRegistry
    {
        string this[string key] { get; }

        Task<bool> AddStaticKeyPathAsync(string keyPath);
        Task AddUpdatingPathAsync(string keyPath);
        void AddWatchOnEntireConfig(Action callback);
        void AddWatchOnSingleKey(string keyToWatch, Action<string> callback);
        Task<bool> SetKeyAsync(string keyPath, string value);
        bool TryGetValue(string key, out string value);
        IEnumerable<string> AllKeys { get; }
        void UpdateKeyParser(IKeyParser parser);
    }
}