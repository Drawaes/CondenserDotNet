using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Host.RoutingTrie
{
    public class ChildrenContainer:IEnumerable<KeyValuePair<string[], Node>>
    {
        public ChildrenContainer(int keyLength)
        {
            _keylength = keyLength;
            _children = new Dictionary<string[], Node>(new NodeComparer(_keylength));

        }

        int _keylength;
        Dictionary<string[], Node> _children;

        public int KeyLength { get { return _keylength; } }

        public Node FindFirstNodeThatMatches(string[] route, int compareLength)
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

        public void Add(string[] route, Node node)
        {
            _children.Add(route, node);
        }

        public ChildrenContainer SplitContainer(int newKeyLength, string currentPath)
        {
            ChildrenContainer newContainer = new ChildrenContainer(newKeyLength);
            foreach(var kv in _children)
            {
                var newPrefix = kv.Key.Take(newKeyLength).ToArray();
                var newTailPrefix = kv.Key.Skip(newKeyLength).ToArray();
                Node newHeadNode;
                if(!newContainer._children.TryGetValue(newPrefix, out newHeadNode))
                {
                    newHeadNode = new Node(newPrefix, currentPath, KeyLength - newKeyLength);
                    newContainer.Add(newPrefix, newHeadNode);
                }
                var oldNode = kv.Value.CloneWithNewPrefix(newTailPrefix, newHeadNode.Path);
                newHeadNode.Children.Add(newTailPrefix, oldNode);
            }

            return newContainer;
        }

        public IEnumerator<KeyValuePair<string[], Node>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        public Node this [string[] key]
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

        public bool TryGetValue(string[] route, out Node node)
        {
            return _children.TryGetValue(route,out node);
        }

        internal ChildrenContainer Clone()
        {
            ChildrenContainer container = new ChildrenContainer(KeyLength);
            container._children = new Dictionary<string[], Node>(_children, new NodeComparer(KeyLength));

            return container;
        }
    }
}
