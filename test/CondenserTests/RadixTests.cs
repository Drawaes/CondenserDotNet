using CondenserDotNet.Server;
using CondenserDotNet.Server.RoutingTrie;
using CondenserTests.Fakes;
using Xunit;

namespace CondenserTests
{
    public class RadixTests
    {
        [Fact]
        public void TestSplitting()
        {
            var tree = new RadixTree<Service>();
            var registry = new FakeServiceRegistry();
            registry.AddServiceInstance("Address1Test", 10000);
            registry.AddServiceInstance("Address2Test", 10000);

            var service = new Service(new string[0], "Service1Test", "node1", new string[0], 
                registry);
            var service2 = new Service(new string[0], "Service1Test", "node1", new string[0], 
                registry);

            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service);

            tree.Compress();

            //Time to split the tree it should be 5 long at the moment but this should make it 2 long and then split
            tree.AddServiceToRoute("/test1/test2/test10/test6/test4/test6", service);

            var topNode = tree.GetTopNode();

            //Check the first node is two long
            Assert.Equal(2, topNode.ChildrenNodes.KeyLength);

            //find the root common node
            var commonNode = topNode.ChildrenNodes[new string[] { "test1", "test2" }];

            //check the keys are consistant
            foreach (var kv in commonNode.ChildrenNodes)
            {
                Assert.Equal(kv.Key.Length, commonNode.ChildrenNodes.KeyLength);
            }
        }

        [Fact]
        public void TestCompression()
        {
            var tree = new RadixTree<Service>();

            var registry = new FakeServiceRegistry();

            registry.AddServiceInstance("Address1Test", 10000);
            registry.AddServiceInstance("Address2Test", 10000);

            var service = new Service(new string[0], "Service1Test", "node1", new string[0], registry);
            var service2 = new Service(new string[0], "Service1Test", "node1", new string[0], registry);

            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service2);

            Assert.Equal(1, tree.GetTopNode().ChildrenNodes.Count);

            tree.Compress();

            ////We should have 1 node in the tree
            Assert.Equal(1, tree.GetTopNode().ChildrenNodes.Count);

            ////The key length should be 5 long
            Assert.Equal(5, tree.GetTopNode().ChildrenNodes.KeyLength);

            string matchedpath;
            var returnservice = tree.GetServiceFromRoute("/test1/test2/test3/test4/test5/test7",out matchedpath);
            Assert.Equal("/test1/test2/test3/test4/test5", matchedpath);
            Assert.Equal(returnservice.ServiceId, service.ServiceId);
        }

        [Fact]
        public void TestRemovingAService()
        {
            var tree = new RadixTree<Service>();

            var registry = new FakeServiceRegistry();

            registry.AddServiceInstance("Address1Test", 10000);
            registry.AddServiceInstance("Address2Test", 10000);

            var service = new Service(new string[0], "Service1Test", "node1", new string[0], registry);
            var service2 = new Service(new string[0], "Service1Test","node1", new string[0], registry);

            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service2);

            string matchedpath;
            var returnservice = tree.GetServiceFromRoute("/test1/test2/test3/test4/test5/test7", out matchedpath);
            Assert.Equal("/test1/test2/test3/test4/test5", matchedpath);

            //now remove the service
            tree.RemoveService(service);

            //Now we should get no match
            returnservice = tree.GetServiceFromRoute("/test1/test2/test3/test4/test5/test7", out matchedpath);
            Assert.Null(returnservice);

        }
    }
}
