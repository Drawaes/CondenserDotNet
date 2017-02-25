using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CondenserDotNet.Services.Consul
{
    public class ConsulServiceRegistration : IServiceRegistration
    {
        public ConsulServiceRegistration()
        {
            ServicePort = GetNextAvailablePort();
            ServiceAddress = Dns.GetHostName();
        }

        public bool UnregisterOnDispose => throw new NotImplementedException();

        public string ServiceId => throw new NotImplementedException();

        public string ServiceName => throw new NotImplementedException();

        public TimeSpan DeregisterIfCriticalAfter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsRegistered => throw new NotImplementedException();

        public ITtlCheck TtlCheck { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ServiceAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ServicePort { get; set; }

        public List<string> SupportedUrls => throw new NotImplementedException();

        public HealthCheck HttpCheck { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected int GetNextAvailablePort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            int port = 0;
            try
            {
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                l.Server.Dispose();
            }
            catch { /*Nom nom */}
            return port;
        }

        public void Dispose()
        {
        }
    }
}
