using System.Collections.Generic;
using Xunit;

namespace CondenserTests
{
    public class RountingStrategyFacts
    {
        private readonly List<RoutedService> _services = new List<RoutedService>()
        {
            new RoutedService() { Id = 1 },
            new RoutedService() { Id = 2 },
        };

        [Fact]
        public void RoundRobinFact()
        {
            var router = new CondenserDotNet.Core.Routing.RoundRobinRoutingStrategy<RoutedService>();
            var service1 = router.RouteTo(_services);
            var service2 = router.RouteTo(_services);
            var service3 = router.RouteTo(_services);

            Assert.Equal(1, service1.Id);
            Assert.Equal(2, service2.Id);
            Assert.Equal(1, service3.Id);
        }

        [Fact]
        public void RoundRobinEmptyListFact()
        {
            var router = new CondenserDotNet.Core.Routing.RoundRobinRoutingStrategy<RoutedService>();
            var service = router.RouteTo(new List<RoutedService>());
            Assert.Null(service);
        }

        [Fact]
        public void RoundRobinNullListFact()
        {
            var router = new CondenserDotNet.Core.Routing.RoundRobinRoutingStrategy<RoutedService>();
            var service = router.RouteTo(null);
            Assert.Null(service);
        }

        private class RoutedService
        {
            public int Id { get; set; }
        }
    }
}
