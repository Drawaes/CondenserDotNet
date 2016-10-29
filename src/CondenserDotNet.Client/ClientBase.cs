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
    public abstract class ClientBase
    {
        protected static HttpClient _httpClient;
        bool _disposed = false;
        


        protected void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Service Registration Client is already disposed");
            }
        }

       
    }
}
