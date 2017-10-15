using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class NodeContainer<T> : IEnumerable<Tuple<string[], Node<T>>>
    {
        private readonly int _keylength;
        private readonly Func<ChildContainer<T>> _factory;
        private readonly NodeComparer _comparer;
        private List<Tuple<string[], Node<T>>> _children;

        public NodeContainer(int keyLength, Func<ChildContainer<T>> factory)
        {
            _keylength = keyLength;
            _comparer = new NodeComparer(_keylength);
            _factory = factory;
            _children = new List<Tuple<string[], Node<T>>>(5);
        }

        public int Count => _children.Count;
        public int KeyLength => _keylength;

        public Node<T> FindFirstNodeThatMatches(string[] route, int compareLength)
        {
            NodeComparer compare;
            if (compareLength != _keylength)
            {
                compare = new NodeComparer(compareLength);
            }
            else
            {
                compare = _comparer;
            }
            foreach (var child in _children)
            {
                if (compare.Equals(child.Item1, route))
                {
                    return child.Item2;
                }
            }
            return null;
        }

        public void Add(string[] route, Node<T> node) => _children.Add(Tuple.Create(route, node));

        public NodeContainer<T> SplitContainer(int newKeyLength, string currentPath)
        {
            var newContainer = new NodeContainer<T>(newKeyLength, _factory);
            foreach (var kv in _children)
            {
                var newPrefix = kv.Item1.Take(newKeyLength).ToArray();
                var newTailPrefix = kv.Item1.Skip(newKeyLength).ToArray();
                if (!newContainer.TryGetValue(newPrefix, out var newHeadNode))
                {
                    newHeadNode = new Node<T>(newPrefix, currentPath, KeyLength - newKeyLength, _factory);
                    newContainer.Add(newPrefix, newHeadNode);
                }
                var oldNode = kv.Item2.CloneWithNewPrefix(newTailPrefix, newHeadNode.Path);
                newHeadNode.ChildrenNodes.Add(newTailPrefix, oldNode);
            }
            return newContainer;
        }

        public bool TryGetValue(string[] searchValue, out Node<T> node)
        {
            foreach (var child in _children)
            {
                if (_comparer.Equals(child.Item1, searchValue))
                {
                    node = child.Item2;
                    return true;
                }
            }
            node = null;
            return false;
        }

        public IEnumerator<Tuple<string[], Node<T>>> GetEnumerator() => _children.GetEnumerator();

        public Node<T> this[string[] key]
        {
            get
            {
                if (TryGetValue(key, out var node))
                {
                    return node;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        public int MaxNodeDepth()
        {
            var nodeDepth = 0;
            foreach (var n in _children)
            {
                nodeDepth = Math.Max(n.Item2.MaxDepth(), nodeDepth);
            }
            return nodeDepth;
        }

        internal NodeContainer<T> Clone()
        {
            var container = new NodeContainer<T>(KeyLength, _factory)
            {
                _children = _children.ToList()
            };
            return container;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
