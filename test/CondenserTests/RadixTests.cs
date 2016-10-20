using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondenserDotNet.Host;
using Xunit;

namespace CondenserTests
{
    public class RadixTests
    {
        [Fact]
        public void TestSplitting()
        {
            CondenserDotNet.Host.RoutingTrie.RadixTree tree = new CondenserDotNet.Host.RoutingTrie.RadixTree();
            Service service = new Service(new string[0], "Service1Test", "Address1Test", 10000, new string[0]);
            Service service2 = new Service(new string[0], "Service1Test", "Address2Test", 10000, new string[0]);

            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service);

            tree.Compress();
            
            //Time to split the tree it should be 5 long at the moment but this should make it 2 long and then split
            tree.AddServiceToRoute("/test1/test2/test10/test6/test4/test6", service);

            var topNode = tree.GetTopNode();

            //Check the first node is two long
            Assert.Equal(2, topNode.Children.KeyLength);

            //find the root common node
            var commonNode = topNode.Children[new string[] { "test1", "test2"}];

            //check the keys are consistant
            foreach(var kv in commonNode.Children)
            {
                Assert.Equal(kv.Key.Length,commonNode.Children.KeyLength);
            }
        }

        [Fact]
        public void TestCompression()
        {
            CondenserDotNet.Host.RoutingTrie.RadixTree tree = new CondenserDotNet.Host.RoutingTrie.RadixTree();
            Service service = new Service(new string[0], "Service1Test", "Address1Test", 10000, new string[0]);
            Service service2 = new Service(new string[0], "Service1Test", "Address2Test", 10000, new string[0]);

            tree.AddServiceToRoute("/test1/test2/test3/test4/test5", service);
            tree.AddServiceToRoute("/test1/test2/test3/test4/test5/test6", service2);

            Assert.Equal(1, tree.GetTopNode().Children.Count);

            tree.Compress();

            //We should have 1 node in the tree
            Assert.Equal(1, tree.GetTopNode().Children.Count);

            //The key length should be 5 long
            Assert.Equal(5, tree.GetTopNode().Children.KeyLength);

            string matchedpath;
            var returnservice = tree.GetServiceFromRoute("/test1/test2/test3/test4/test5/test7",out matchedpath);
            Assert.Equal("/test1/test2/test3/test4/test5", matchedpath);
            Assert.Equal(returnservice.ServiceId, service.ServiceId);
        }
    }
}
