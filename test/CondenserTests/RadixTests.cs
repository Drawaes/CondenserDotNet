using System;
using CondenserDotNet.Core.Routing;
using CondenserDotNet.Server;
using CondenserDotNet.Server.RoutingTrie;
using Xunit;

namespace CondenserTests
{
    public class RadixTests
    {
        [Fact(Skip ="Ignore while compression in flux")]
        public void TestSplitting()
        {
            var tree = CreateDefault();

            var routingData = new RoutingData(null);
            var service = new Service(null, null, routingData);
            service.Initialise("Service1Test", "node1", new string[0], "Adress1Test", 10000).Wait();
            var service2 = new Service(null, null, routingData);
            service2.Initialise("Service1Test", "node1", new string[0], "Address2Test", 10000).Wait();
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service);

            tree.Compress();

            ////Time to split the tree it should be 5 long at the moment but this should make it 2 long and then split
            tree.AddServiceToRoute("/test1/test2/test10/test6/test4/test6", service);

            var topNode = tree.GetTopNode();

            //Check the first node is two long
            Assert.Equal(2, topNode.ChildrenNodes.KeyLength);

            //find the root common node
            var commonNode = topNode.ChildrenNodes[new string[] { "TEST1", "TEST2" }];

            //check the keys are consistant
            foreach (var kv in commonNode.ChildrenNodes)
            {
                Assert.Equal(kv.Item1.Length, commonNode.ChildrenNodes.KeyLength);
            }
        }

        [Fact(Skip = "Ignore while compression in flux")]
        public void TestCompression()
        {
            var tree = CreateDefault();

            var routingData = new RoutingData(null);
            var service = new Service(null, null, routingData);
            service.Initialise("Service1Test", "node1", new string[0], "Address1Test", 10000).Wait();
            var service2 = new Service(null, null, routingData);
            service2.Initialise("Service1Test", "node1", new string[0], "Address2Test", 10000).Wait();

            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service2);

            Assert.Equal(1, tree.GetTopNode().ChildrenNodes.Count);

            tree.Compress();

            //We should have 1 node in the tree
            Assert.Equal(1, tree.GetTopNode().ChildrenNodes.Count);

            //The key length should be 5 long
            Assert.Equal(5, tree.GetTopNode().ChildrenNodes.KeyLength);

            var returnservice = tree.GetServiceFromRoute("/test1/test2/test3/test4/test5/test7", out var matchedpath);
            Assert.Equal("/test1/test2/test3/test4/test5".ToUpperInvariant(), matchedpath);
            //Assert.Equal(returnservice.ServiceId, service.ServiceId);
        }

        [Fact]
        public void TestRemovingAService()
        {
            var tree = CreateDefault();

            var routingData = new RoutingData(null);
            var service = new Service(null, null, routingData);
            service.Initialise("Service1Test", "node1", new string[0], "Address1Test", 10000).Wait();
            var service2 = new Service(null, null, routingData);
            service2.Initialise("Service1Test", "node1", new string[0], "Address2Test", 10000).Wait();

            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service2);

            var returnservice = tree.GetServiceFromRoute("/test1/test2/test3/test4/test5/test7", out var matchedpath);
            Assert.Equal("/test1/test2/test3/test4/test5".ToUpperInvariant(), matchedpath);

            ////now remove the service
            tree.RemoveService(service);

            //Now we should get no match
            returnservice = tree.GetServiceFromRoute("/test1/test2/test3/test4/test5/test7", out matchedpath);
            Assert.Null(returnservice);

        }

        public static RadixTree<IService> CreateDefault() =>
            new RadixTree<IService>(() =>
            {
                var randomRoutingStrategy = new RandomRoutingStrategy<IService>();
                return new ChildContainer<IService>(new DefaultRouting<IService>
                    (new[] { randomRoutingStrategy }, new FakeRouteConfig()));
            });
    }

    public class FakeRouteConfig : IRoutingConfig
    {
        public string DefaultRouteStrategy { get; } = RouteStrategy.Random.ToString();

        public Action<string[]> OnRoutesBuilt => null;
    }
}
