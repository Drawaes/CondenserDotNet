using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CondenserDotNet.Service;
using Microsoft.AspNetCore.Routing;

namespace CondenserDotNet.Server
{
    public class RoutingHost
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly CustomRouter _router;
        private readonly ServiceRegistry _services;

        public RoutingHost(CustomRouter router,
            CondenserConfiguration configuration)
        {
            _router = router;
            var uri = new Uri($"http://{configuration.AgentAddress}:{configuration.AgentPort}");
            var client = new HttpClient { BaseAddress = uri };

            _services = new ServiceRegistry(client, _cancel.Token);
        }

        public IRouter Router => _router;

        public async Task<bool> Initialise()
        {
            var serviceDescriptions = await _services.GetAvailableServicesWithTagsAsync();

            foreach (var description in serviceDescriptions)
            {
                var tags = description.Value
                    .Select(x => x.Replace("urlprefix-",""))
                    .ToArray();

                var name = description.Key;

                var service = new Service(tags,
                    name, name, tags, _services);

                _router.AddNewService(service);
            }

            return true;
        }
    }
}