using System;
using CondenserDotNet.Client.DataContracts;


namespace CondenserDotNet.Client
{
    public class HealthConfiguration
    {
        public bool IgnoreTls { get; set; } = true;
        public string Url { get; set; }
        public int IntervalInSeconds { get; set; } = 30;

        public HealthCheck Build(IServiceManager manager)
        {
            if (Url == null) return null;
            if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            {
                var scheme = manager.ProtocolSchemeTag ?? "http";
                var builder = new UriBuilder(scheme, manager.ServiceAddress, manager.ServicePort, Url);
                uri = builder.Uri;
            }

            var check = new HealthCheck
            {
                HTTP = uri.ToString(),
                Interval = $"{IntervalInSeconds}s",
                Name = $"{manager.ServiceId}:HttpCheck",
                tls_skip_verify = IgnoreTls
            };

            return check;
        }
    }
}
