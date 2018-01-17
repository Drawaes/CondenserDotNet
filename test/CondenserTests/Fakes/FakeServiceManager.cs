using CondenserDotNet.Client;
using System;
using System.Collections.Generic;
using System.Text;
using CondenserDotNet.Client.DataContracts;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CondenserTests.Fakes
{
    public class FakeServiceManager : IServiceManager
    {
        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public TimeSpan DeregisterIfCriticalAfter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsRegistered => throw new NotImplementedException();

        public ITtlCheck TtlCheck { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string ServiceAddress { get; set; }

        public int ServicePort { get; set; }

        public CancellationToken Cancelled => throw new NotImplementedException();

        public ILogger Logger => null;

        public HttpClient Client => throw new NotImplementedException();

        public List<string> SupportedUrls => throw new NotImplementedException();

        public HealthConfiguration HealthConfig { get; } = new HealthConfiguration();

        public Service RegisteredService { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ProtocolSchemeTag { get; set; }

        public void Dispose() => throw new NotImplementedException();
    }
}
