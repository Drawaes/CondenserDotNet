using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Host.RoutingTrie
{
    public class RadixTree
    {
        Node _topNode = new Node(new string[0], "");
        object _writeLock = new object();
        char[] _routeSplit = new char[] {'/'};

        public void AddServiceToRoute(string route, Service service)
        {
            lock (_writeLock)
            {
                _topNode.AddRoute(route.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries), service);
            }
        }

        public Service GetServiceFromRoute(string route, out string matchedPath)
        {
            return _topNode.GetService(route.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries), out matchedPath);
        }

        public void Compress()
        {
            lock (_writeLock)
            {
                _topNode.Compress();
            }
        }
        
        public void RemoveService(Service service)
        {
            lock (_writeLock)
            {
                _topNode.RemoveService(service);
            }
        }

        public void RemoveServiceFromRoute(string route, Service service)
        {
            lock (_writeLock)
            {
                _topNode.RemoveServiceFromRoute(route.Split(_routeSplit, StringSplitOptions.RemoveEmptyEntries), service);
            }
        }

        public Node GetTopNode()
        {
            return _topNode;
        }
    }
}