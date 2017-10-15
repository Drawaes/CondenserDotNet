using System.Collections.Generic;
using System.Threading;
using CondenserDotNet.Core.Routing;
using System;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class ChildContainer<T>
    {
        private IRoutingStrategy<T> _routingStrategy;
        private List<T> _services = new List<T>();

        public ChildContainer(IDefaultRouting<T> defaultStrategy) => _routingStrategy = defaultStrategy.Default;

        public int Count => Volatile.Read(ref _services).Count;

        public void SetRoutingStrategy(IRoutingStrategy<T> routingStrategy, Func<List<T>, bool> applies)
        {
            var services = Volatile.Read(ref _services);

            if (applies(services))
            {
                Interlocked.Exchange(ref _routingStrategy, routingStrategy);
            }

        }
        public override string ToString() => $"Total Services Registered {_services.Count}";

        public void AddService(T service)
        {
            var newServices = new List<T>(Volatile.Read(ref _services))
            {
                service
            };
            Volatile.Write(ref _services, newServices);
        }

        public bool RemoveService(T service)
        {
            var newServices = new List<T>(Volatile.Read(ref _services));
            var result = newServices.Remove(service);
            Volatile.Write(ref _services, newServices);
            return result;
        }

        public T GetService()
        {
            var services = Volatile.Read(ref _services);
            if (services.Count > 0)
            {
                //Simple random selector
                return _routingStrategy.RouteTo(services);
            }
            return default;
        }
    }
}
