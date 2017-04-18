using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CondenserDotNet.Configuration
{
    public interface IConfigurationRegistry : IDisposable
    {
        string this[string key] { get; }
        Task<bool> AddStaticKeyPathAsync(string keyPath);
        Task AddUpdatingPathAsync(string keyPath);
        void AddWatchOnEntireConfig(Action callback);
        void AddWatchOnSingleKey(string keyToWatch, Action<string> callback);
        Task<bool> SetKeyAsync(string keyPath, string value);
        bool TryGetValue(string key, out string value);
        IEnumerable<string> AllKeys { get; }
        IConfigurationRoot Root { get; }
    }
}
