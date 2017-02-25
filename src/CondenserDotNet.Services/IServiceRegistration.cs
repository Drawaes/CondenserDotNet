using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Services
{
    public interface IServiceRegistration:IDisposable
    {
        bool UnregisterOnDispose { get; }
        string ServiceId { get; }
        string ServiceName { get; }
        TimeSpan DeregisterIfCriticalAfter { get; set; }
        bool IsRegistered { get; }
        ITtlCheck TtlCheck { get; set; }
        string ServiceAddress { get; set; }
        int ServicePort { get; set; }
        List<string> SupportedUrls { get; }
        HealthCheck HttpCheck { get; set; }
    }
}
