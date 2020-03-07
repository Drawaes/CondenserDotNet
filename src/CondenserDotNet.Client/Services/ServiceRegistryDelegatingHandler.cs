using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Services
{
    public class ServiceRegistryDelegatingHandler : DelegatingHandler
    {
        private readonly IServiceRegistry _serviceRegistry;

        public ServiceRegistryDelegatingHandler(IServiceRegistry serviceRegistry) => _serviceRegistry = serviceRegistry;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentUri = request.RequestUri;
            if (System.Net.IPAddress.TryParse(currentUri.Host, out _)) return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var serviceInstance = await _serviceRegistry.GetServiceInstanceAsync(currentUri.Host).ConfigureAwait(false);
            if (serviceInstance == null)
            {
                throw new NoServiceInstanceFoundException(currentUri.Host, null);
            }
            request.RequestUri = new Uri($"{currentUri.Scheme}://{serviceInstance.Address}:{serviceInstance.Port}{currentUri.PathAndQuery}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
