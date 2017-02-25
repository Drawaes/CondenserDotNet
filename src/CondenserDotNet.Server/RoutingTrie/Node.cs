using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Server.RoutingTrie
{
    public class Node<T>
    {
        private readonly Func<ChildContainer<T>> _factory;
        private NodeContainer<T> _childrenNodes;
        
        public Node(string[] prefix, string pathToHere, Func<ChildContainer<T>> factory)
            :this(prefix, pathToHere, 1, factory)
        {
        }

        public Node(string[] prefix, string pathToHere,int initialKeySize,
            Func<ChildContainer<T>> factory)
        {
            _factory = factory;

            Services = factory();
            Prefix = prefix;
            _childrenNodes = new NodeContainer<T>(initialKeySize, _factory);
            Path = pathToHere + "/" + string.Join("/", Prefix);
            if(Path == "/")
                Path = "";
        }
        
        public string Path { get; private set;}
        public ChildContainer<T> Services { get; private set; }
        public NodeContainer<T> ChildrenNodes => _childrenNodes;
        public string[] Prefix { get; private set; }
                
        public Node<T> CloneWithNewPrefix(string[] newPrefix, string newPath)
        {
            Node<T> newNode = new Node<T>(newPrefix, newPath, _factory);
            newNode._childrenNodes = _childrenNodes;
            newNode.Services = Services;

            return newNode;
        }

        public void AddRoute(string[] route, T service)
        {
            if(route.Length == 0)
            {
                Services.AddService(service);
                return;
            }
            var children = System.Threading.Volatile.Read(ref _childrenNodes);

            for (int i = Math.Min(children.KeyLength, route.Length); i > 0;  i--)
            {
                //We need to first see if the first part of the route matches any of the current nodes
                var matche = children.FindFirstNodeThatMatches(route, i);
                if (matche != null)
                {
                    //we found something that matched our prefix, if the key length is a match then just pass down the service to the next node
                    if (children.KeyLength == i)
                    {
                        matche.AddRoute(route.Skip(i).ToArray(), service);
                        return;
                    }
                    else
                    {
                        var newChildren = children.SplitContainer(i, Path);
                        newChildren[route].AddRoute(route.Skip(i).ToArray(), service);
                        System.Threading.Volatile.Write(ref _childrenNodes, newChildren);
                        return;
                    }
                }
            }
            //Nothing matched, if we have a key >= current prefix length we can just add it, otherwise we need a split
            if(route.Length >= children.KeyLength)
            {
                //Create a new node and add it
                Node<T> n = new Node<T>(route.Take(children.KeyLength).ToArray(), Path, _factory);
                n.AddRoute(route.Skip(children.KeyLength).ToArray(),service);

                var newChildren = children.Clone();
                newChildren.Add(n.Prefix, n);
                System.Threading.Volatile.Write(ref _childrenNodes, newChildren);
                return;
            }
            else
            {
                //The key is smaller than the current key length so we are going to have to split before we add
                var newChildren = ChildrenNodes.SplitContainer(route.Length, Path);
                Node<T> n = new Node<T>(route, Path, _factory);
                n.AddRoute(new string[0], service);
                newChildren.Add(n.Prefix, n);

                System.Threading.Volatile.Write(ref _childrenNodes, newChildren);
                return;
            }
        }

        public int MaxDepth()
        {
            return _childrenNodes.MaxNodeDepth() + 1;
        }

        internal bool RemoveServiceFromRoute(string[] route, T service)
        {
            NodeContainer<T> container = System.Threading.Volatile.Read(ref _childrenNodes);
            if (route.Length == 0)
            {
                return Services.RemoveService(service);
            }

            Node<T> child;
            if (container.TryGetValue(route, out child))
            {
                return child.RemoveServiceFromRoute(route.Skip(container.KeyLength).ToArray(), service);
            }
            return false;
        }

        internal void RemoveService(T service)
        {
            var children = System.Threading.Volatile.Read(ref _childrenNodes);
            Services.RemoveService(service);
            foreach(var kv in children)
            {
                kv.Item2.RemoveService(service);
            }
        }

        public T GetService(string[] route, out string matchedPath)
        {
            var container = System.Threading.Volatile.Read(ref _childrenNodes);
            if(route.Length == 0)
            {
                matchedPath = Path;
                return Services.GetService();
            }

            Node<T> child;
            if(container.TryGetValue(route, out child))
            {
                var returnService = child.GetService(route.Skip(container.KeyLength).ToArray(), out matchedPath);
                if(returnService != null)
                {
                    return returnService;
                }
            }
            matchedPath = Path;
            return Services.GetService();
        }
               
        public void Compress()
        {
            bool canCompress = true;

            var children = System.Threading.Volatile.Read(ref _childrenNodes);

            if(children.Count == 0) return;
            foreach (var kv in children)
            {
                if (kv.Item2.Services.Count > 0)
                {
                    canCompress = false;
                    break;
                }
            }

            if (canCompress)
            {
                var newMerged = new NodeContainer<T>(children.KeyLength + 1, _factory);
                foreach (var kv in children)
                {
                    foreach (var childkv in kv.Item2.ChildrenNodes)
                    {
                        //This will prune out orphaned trees
                        if (childkv.Item2.Services.Count > 0 || childkv.Item2.ChildrenNodes.Count > 0)
                        {
                            var mergedKey = Enumerable.Concat(kv.Item1, childkv.Item1).ToArray();
                            var newNode = childkv.Item2.CloneWithNewPrefix(mergedKey, Path);
                            newMerged.Add(newNode.Prefix, newNode);
                        }
                    }
                }
                System.Threading.Volatile.Write(ref _childrenNodes, newMerged);
                Compress();
            }
            else
            {
                foreach (var kv in children)
                {
                    kv.Item2.Compress();
                }
            }
        }

        public override string ToString()
        {
            return $"Path {string.Join("/",Prefix)} Services {Services.Count}";
        }
    }
}
