using System;
using System.Collections.Generic;
using System.Text;

namespace CondenserDotNet.Core.Routing
{
    public class UseTopRouting<T> : IRoutingStrategy<T>
    {
        public static UseTopRouting<T> Default { get; } = new UseTopRouting<T>();

        public string Name => "UseTopRouting";

        public T RouteTo(List<T> services) => services?.Count > 0 ? services[0] : default;
    }
}
