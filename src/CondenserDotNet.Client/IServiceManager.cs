using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CondenserDotNet.Client.DataContracts;
using Microsoft.Extensions.Logging;

namespace CondenserDotNet.Client
{
    public interface IServiceManager : IDisposable
    {
        string ServiceId { get; }
        string ServiceName { get; }
        TimeSpan DeregisterIfCriticalAfter { get; set; }
        bool IsRegistered { get; }
        ITtlCheck TtlCheck { get; set; }
        string ServiceAddress { get; }
        int ServicePort { get; }
        CancellationToken Cancelled { get; }
        HttpClient Client { get; }
        List<string> SupportedUrls { get; }
        HealthConfiguration HealthConfig { get; }
        Service RegisteredService { get; set; }
        string ProtocolSchemeTag { get; set; }
        ILogger Logger { get; }
    }
}
