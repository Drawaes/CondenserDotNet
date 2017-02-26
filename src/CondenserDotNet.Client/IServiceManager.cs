using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using CondenserDotNet.Client.DataContracts;
using CondenserDotNet.Client.Services;
using CondenserDotNet.Core;

namespace CondenserDotNet.Client
{
    public interface IServiceManager : IDisposable
    {
        string ServiceId { get; }
        string ServiceName { get; }
        TimeSpan DeregisterIfCriticalAfter { get; set; }
        IServiceRegistry Services { get; }
        bool IsRegistered { get; }
        ITtlCheck TtlCheck { get; set; }
        string ServiceAddress { get; set; }
        int ServicePort { get; set; }
        CancellationToken Cancelled { get; }
        ILeaderRegistry Leaders { get; }
        HttpClient Client { get; }
        List<string> SupportedUrls { get; }
        HealthCheck HttpCheck { get; set; }
        Service RegisteredService { get; set; }
    }
}