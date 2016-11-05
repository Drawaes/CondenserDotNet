using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class RadixTree<T>
    {
        private readonly Node<T> _topNode = new Node<T>(new string[0], "");
        private readonly object _writeLock = new object();
        private static readonly char[] _routeSplit = new char[] {'/'};

        public void AddServiceToRoute(string route, T service)
        {
            lock (_writeLock)
            {
                _topNode.AddRoute(route.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries), service);
            }
        }

        public T GetServiceFromRoute(string route, out string matchedPath)
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
        
        public void RemoveService(T service)
        {
            lock (_writeLock)
            {
                _topNode.RemoveService(service);
            }
        }

        public void RemoveServiceFromRoute(string route, T service)
        {
            lock (_writeLock)
            {
                _topNode.RemoveServiceFromRoute(route.Split(_routeSplit, StringSplitOptions.RemoveEmptyEntries), service);
            }
        }

        public Node<T> GetTopNode()
        {
            return _topNode;
        }
    }
}