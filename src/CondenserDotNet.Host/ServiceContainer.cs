using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.Host
{
    public class ServiceContainer
    {
        static int seed = Environment.TickCount;

        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
        
        List<Service> _services = new List<Service>();
        
        public void AddService(Service service)
        {
            var newServices = new List<Service>(Volatile.Read(ref _services));
            newServices.Add(service);
            Volatile.Write(ref _services, newServices);
        }

        public bool RemoveService(Service service)
        {
            var newServices = new List<Service>(Volatile.Read(ref _services));
            var result = newServices.Remove(service);
            Volatile.Write(ref _services, newServices);
            return result;
        }

        public Service GetService()
        {
            var services = Volatile.Read(ref _services);
            if (services.Count > 0)
            {
                //Simple random selector
                return services[random.Value.Next(0, services.Count)];
            }
            return null;
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
