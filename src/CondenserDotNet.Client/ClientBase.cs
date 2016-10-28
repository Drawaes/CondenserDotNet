using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    public abstract class ClientBase : IDisposable
    {
        protected static HttpClient _httpClient;
        bool _disposed = false;
        
        public ClientBase() :this("127.0.0.1", 8500) { }
        public ClientBase(int agentPort) :this("127.0.0.1", agentPort) { }
        public ClientBase(string agentAddress) :this(agentAddress, 8500) { }
        public ClientBase(string agentAddress, int agentPort)
        {
            
        }

        protected void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Service Registration Client is already disposed");
            }
        }

        #region Supports IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_disposed)
            {
            }

            _httpClient.Dispose();
            _disposed = true;
        }

        ~ClientBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
