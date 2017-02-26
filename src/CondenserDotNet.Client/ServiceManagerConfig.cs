using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Net;
using System.Net.Sockets;

namespace CondenserDotNet.Client
{
    public class ServiceManagerConfig
    {
        public ServiceManagerConfig()
        {
            ServiceAddress = Dns.GetHostName();
            ServiceName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            ServiceId = $"{ServiceName}:{ServiceAddress}";
        }

        public string ServiceName { get;set;}
        public string ServiceId { get; set; }
        public string ServiceAddress { get; set; }
        public int ServicePort { get;set; }
        
        public static int GetNextAvailablePort()
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
    }
}
