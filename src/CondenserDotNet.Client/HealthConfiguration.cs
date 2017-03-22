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
            if (Url == null)
                return null;

            string scheme = manager.ProtocolSchemeTag ?? "http";

            var check = new HealthCheck
            {
                HTTP = $"{scheme}://{manager.ServiceAddress}:{manager.ServicePort}{Url}",
                Interval = $"{IntervalInSeconds}s",
                Name = $"{manager.ServiceId}:HttpCheck",
                tls_skip_verify = IgnoreTls
            };

            return check;
        }
    }
}
