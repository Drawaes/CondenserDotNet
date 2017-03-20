using System;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class RadixTree<T>
    {
        private readonly Node<T> _topNode;
        private readonly object _writeLock = new object();
        private static readonly char[] _routeSplit = new char[] { '/' };
        private readonly bool _killCase = true;

        public RadixTree(Func<ChildContainer<T>> factory) => _topNode = new Node<T>(new string[0], "", factory);

        public Node<T> TopNode => _topNode;

        public void AddServiceToRoute(string route, T service)
        {
            if (_killCase)
            {
                route = route.ToUpperInvariant();
            }
            lock (_writeLock)
            {
                _topNode.AddRoute(route.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries), service);
            }
        }

        public T GetServiceFromRoute(string route, out string matchedPath)
        {
            if (_killCase)
            {
                route = route.ToUpperInvariant();
            }
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
            if (_killCase)
            {
                route = route.ToUpperInvariant();
            }
            lock (_writeLock)
            {
                _topNode.RemoveServiceFromRoute(route.Split(_routeSplit, StringSplitOptions.RemoveEmptyEntries), service);
            }
        }

        public Node<T> GetTopNode() => _topNode;
        public int MaxDepth() => _topNode.MaxDepth() + 1;
    }
}