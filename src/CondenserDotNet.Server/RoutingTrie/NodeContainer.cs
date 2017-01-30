using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class NodeContainer<T>:IEnumerable<KeyValuePair<string[], Node<T>>>
    {
        public NodeContainer(int keyLength, 
            Func<ChildContainer<T>> factory)
        {
            _keylength = keyLength;
            _factory = factory;
            _children = new Dictionary<string[], Node<T>>(new NodeComparer(_keylength));
        }

        int _keylength;
        private readonly Func<ChildContainer<T>> _factory;
        Dictionary<string[], Node<T>> _children;

        public int KeyLength { get { return _keylength; } }

        public Node<T> FindFirstNodeThatMatches(string[] route, int compareLength)
        {
            NodeComparer compare = new NodeComparer(compareLength);
            foreach(var kv in _children)
            {
                if(compare.Equals(kv.Key, route))
                {
                    return kv.Value;
                }
            }
            return null;
        }

        public void Add(string[] route, Node<T> node)
        {
            _children.Add(route, node);
        }

        public NodeContainer<T> SplitContainer(int newKeyLength, string currentPath)
        {
            var newContainer = new NodeContainer<T>(newKeyLength, _factory);
            foreach(var kv in _children)
            {
                var newPrefix = kv.Key.Take(newKeyLength).ToArray();
                var newTailPrefix = kv.Key.Skip(newKeyLength).ToArray();
                Node<T> newHeadNode;
                if(!newContainer._children.TryGetValue(newPrefix, out newHeadNode))
                {
                    newHeadNode = new Node<T>(newPrefix, currentPath, KeyLength - newKeyLength, _factory);
                    newContainer.Add(newPrefix, newHeadNode);
                }
                var oldNode = kv.Value.CloneWithNewPrefix(newTailPrefix, newHeadNode.Path);
                newHeadNode.ChildrenNodes.Add(newTailPrefix, oldNode);
            }

            return newContainer;
        }

        public IEnumerator<KeyValuePair<string[], Node<T>>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        public Node<T> this [string[] key]
        {
            get
            {
                return _children[key];
            }
            set
            {
                _children[key] = value;
            }
        }

        public int Count
        {
            get { return _children.Count;}
        }

        public bool TryGetValue(string[] route, out Node<T> node)
        {
            return _children.TryGetValue(route,out node);
        }

        public int MaxNodeDepth()
        {
            int nodeDepth = 0;
            foreach(var n in _children.Values)
            {
                nodeDepth = Math.Max(n.MaxDepth(), nodeDepth);
            }
            return nodeDepth;
        }

        internal NodeContainer<T> Clone()
        {
            var container = new NodeContainer<T>(KeyLength, _factory);
            container._children = new Dictionary<string[], Node<T>>(_children, new NodeComparer(KeyLength));

            return container;
        }
    }
}
