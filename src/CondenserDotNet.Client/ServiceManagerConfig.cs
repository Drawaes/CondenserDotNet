using System;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;

namespace CondenserDotNet.Client
{
    public class ServiceManagerConfig
    {
        public string ServiceName { get; set; }
        public string ServiceId { get; set; }
        public string ServiceAddress { get; set; }
        public int ServicePort { get; set; }
        public string TagForScheme { get; set; } = "Protocol";
        public Tuple<int, string>[] PortAndSchemes { get; set; }
        public string[] EndsToStrip { get; set; } = { ".Host", ".Library", ".Service" };
        public bool UseMultipleProtocols { get; set; } = true;

        public static int GetNextAvailablePort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            var port = 0;
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

        internal void SetDefaults(IServer server)
        {
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                var assemblyName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                foreach (var strip in EndsToStrip ?? new string[0])
                {
                    if (assemblyName.EndsWith(strip))
                    {
                        assemblyName = assemblyName.Substring(0, assemblyName.Length - strip.Length);
                    }
                }
                ServiceName = assemblyName;
            }
            if (string.IsNullOrWhiteSpace(ServiceAddress))
            {
                var hostName = Dns.GetHostEntryAsync(string.Empty);
                hostName.Wait();
                var name = hostName.Result.HostName;
                ServiceAddress = name;
            }
            if (string.IsNullOrWhiteSpace(ServiceId))
            {
                ServiceId = $"{ServiceName}:{Dns.GetHostName()}";
            }
            if (ServicePort == 0)
            {
                var feature = server.Features.Get<IServerAddressesFeature>();
                var add = feature.Addresses.First().Replace("*", "test");
                ServicePort = new Uri(add).Port;
            }
        }
    }
}
