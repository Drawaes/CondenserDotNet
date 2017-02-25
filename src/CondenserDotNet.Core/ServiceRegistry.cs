using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Core.DataContracts;
using CondenserDotNet.Core.Routing;
using Newtonsoft.Json;

namespace CondenserDotNet.Core
{
    public class ServiceRegistry : IServiceRegistry
    {
        
        

        public ServiceRegistry(
            HttpClient client, CancellationToken cancel)
        {
            _client = client;
            _cancel = cancel;
        }

        
        

        
    }
}
