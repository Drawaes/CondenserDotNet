using System.Collections.Generic;
using System.Threading;
using CondenserDotNet.Core;
using CondenserDotNet.Core.Routing;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class ChildContainer<T>
    {
        private IRoutingStrategy<T> _routingStrategy;
        
        public ChildContainer(IDefaultRouting<T> defaultStrategy)
        {
            _routingStrategy = defaultStrategy.Default;
        }


        List<T> _services = new List<T>();

        public void SetRoutingStrategy(IRoutingStrategy<T> routingStrategy)
        {
            _routingStrategy = routingStrategy;
        }

        public void AddService(T service)
        {
            var newServices = new List<T>(Volatile.Read(ref _services));
            newServices.Add(service);
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
            return default(T);
        }

        public int Count
        {
            get
            {
                return Volatile.Read(ref _services).Count;
            }
        }

        public override string ToString()
        {
            return $"Total Services Registered {_services.Count}";
        }
    }
}


