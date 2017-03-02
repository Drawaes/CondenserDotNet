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
        private readonly static string[] _endsToStrip = {".Host", ".Library", ".Service"};

        public ServiceManagerConfig()
        {
            var ipAwait = Dns.GetHostAddressesAsync(Dns.GetHostName());
            ipAwait.Wait();
            foreach(var ipAddress in ipAwait.Result)
            {
                if(ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServiceAddress = ipAddress.ToString();
                    break;
                }
            }
            var assemblyName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            foreach(var strip in _endsToStrip)
            {
                if(assemblyName.EndsWith(strip))
                {
                    assemblyName.Substring(0,assemblyName.Length - strip.Length);
                }
            }
            ServiceName = assemblyName;
            ServiceId = $"{ServiceName}:{Dns.GetHostName()}";
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
