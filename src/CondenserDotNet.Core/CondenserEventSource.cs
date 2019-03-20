using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace CondenserDotNet.Core
{
    [EventSource(Name = "Condenser-Event-Source")]
    public sealed class CondenserEventSource:EventSource
    {
        public static readonly CondenserEventSource Log = new CondenserEventSource();

        [Event(1, Message = "Service-Watcher-Created")]
        public void ServiceWatcherCreated(string serviceName) => Log.WriteEvent(1, serviceName);

        [Event(2, Message = "Http-Client-Created")]
        public void HttpClientCreated() => Log.WriteEvent(2);

        [Event(3, Message = "Leadership-Session-Created")]
        public void LeadershipSessionCreated() => Log.WriteEvent(3);

        [Event(4, Message = "Leadership-Get-Status")]
        public void LeadershipSessionGetStatus(string keyPath) => Log.WriteEvent(4, keyPath);

        [Event(5, Message = "Leadership-Try-To-Lock")]
        public void LeadershipTryToLock(string keyPath) => Log.WriteEvent(5, keyPath);

        [Event(6, Message = "Configuration-HttpClient-Created")]
        public void ConfigurationHttpClientCreated() => Log.WriteEvent(6);

        [Event(7, Message = "Configuration-Get-Keys-Recursive")]
        public void ConfigurationGetKeysRecursive(string keyPath) => Log.WriteEvent(7, keyPath);

        [Event(8, Message = "Configuration-Get-Key")]
        public void ConfigurationGetKey(string keyPath) => Log.WriteEvent(8, keyPath);

        [Event(9, Message = "Configuration-Watch-Key")]
        public void ConfigurationWatchKey(string keyPath) => Log.WriteEvent(9, keyPath);

        [Event(10, Message = "Configuration-Set-Key")]
        public void ConfigurationSetKey(string keyPath) => Log.WriteEvent(10, keyPath);

        [Event(11, Message = "Service-Registration")]
        public void ServiceRegistration() => Log.WriteEvent(11);
    }
}
