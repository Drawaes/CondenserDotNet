using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Services
{
    public class ServiceBasedHttpHandler : HttpClientHandler
    {
        private readonly IServiceRegistry _serviceRegistry;

        public ServiceBasedHttpHandler(IServiceRegistry serviceRegistry, int maxConnectionsPerServer)
        {
            _serviceRegistry = serviceRegistry;
            MaxConnectionsPerServer = maxConnectionsPerServer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentUri = request.RequestUri;
            var serviceInstance = await _serviceRegistry.GetServiceInstanceAsync(currentUri.Host);
            if (serviceInstance == null)
            {
                throw new NoServiceInstanceFoundException(currentUri.Host, null);
            }
            request.RequestUri = new Uri($"{currentUri.Scheme}://{serviceInstance.Address}:{serviceInstance.Port}{currentUri.PathAndQuery}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
