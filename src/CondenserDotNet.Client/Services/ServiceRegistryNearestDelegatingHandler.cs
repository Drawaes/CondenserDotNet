using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Client.Services
{
    public class ServiceRegistryNearestDelegatingHandler : DelegatingHandler
    {
        private IServiceRegistry _serviceRegistry;

        public ServiceRegistryNearestDelegatingHandler(IServiceRegistry serviceRegistry) => _serviceRegistry = serviceRegistry;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentUri = request.RequestUri;
            var serviceInstance = await _serviceRegistry.GetNearestServiceInstanceAsync(currentUri.Host);
            if (serviceInstance == null)
            {
                throw new NoServiceInstanceFoundException(currentUri.Host, null);
            }
            request.RequestUri = new Uri($"{currentUri.Scheme}://{serviceInstance.Address}:{serviceInstance.Port}{currentUri.PathAndQuery}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
