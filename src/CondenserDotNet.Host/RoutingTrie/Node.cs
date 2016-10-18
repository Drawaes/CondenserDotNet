using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondenserDotNet.Host.RoutingTrie
{
    public class Node
    {
        public Node(string[] prefix, string pathToHere)
            :this(prefix, pathToHere, 1)
        {

        }

        public Node(string[] prefix, string pathToHere,int initialKeySize)
        {
            Prefix = prefix;
            _children = new ChildrenContainer(initialKeySize);
            Path = pathToHere + "/" + string.Join("/", Prefix);
            if(Path == "/")
                Path = "";
        }
        
        public string Path { get; private set;}

        public ServiceContainer Services { get; private set;} = new ServiceContainer();

        ChildrenContainer _children;
        public ChildrenContainer Children {  get {  return _children;} }
        public string[] Prefix { get; private set; }
                
        public Node CloneWithNewPrefix(string[] newPrefix, string newPath)
        {
            Node newNode = new Node(newPrefix, newPath);
            newNode._children = _children;
            newNode.Services = Services;

            return newNode;
        }

        public void AddRoute(string[] route, Service service)
        {
            if(route.Length == 0)
            {
                Services.AddService(service);
                return;
            }
            var children = System.Threading.Volatile.Read(ref _children);

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
                        System.Threading.Volatile.Write(ref _children, newChildren);
                        return;
                    }
                }
            }
            //Nothing matched, if we have a key >= current prefix length we can just add it, otherwise we need a split
            if(route.Length >= children.KeyLength)
            {
                //Create a new node and add it
                Node n = new Node(route.Take(children.KeyLength).ToArray(), Path);
                n.AddRoute(route.Skip(children.KeyLength).ToArray(),service);

                var newChildren = children.Clone();
                newChildren.Add(n.Prefix, n);
                System.Threading.Volatile.Write(ref _children, newChildren);
                return;
            }
            else
            {
                //The key is smaller than the current key length so we are going to have to split before we add
                var newChildren = Children.SplitContainer(route.Length, Path);
                Node n = new Node(route, Path);
                n.AddRoute(new string[0], service);
                newChildren.Add(n.Prefix, n);

                System.Threading.Volatile.Write(ref _children, newChildren);
                return;
            }
        }

        internal bool RemoveServiceFromRoute(string[] route, Service service)
        {
            ChildrenContainer container = System.Threading.Volatile.Read(ref _children);
            if (route.Length == 0)
            {
                return Services.RemoveService(service);
            }

            Node child;
            if (container.TryGetValue(route, out child))
            {
                return child.RemoveServiceFromRoute(route.Skip(container.KeyLength).ToArray(), service);
            }
            return false;
        }

        internal void RemoveService(Service service)
        {
            var children = System.Threading.Volatile.Read(ref _children);
            Services.RemoveService(service);
            foreach(var kv in children)
            {
                kv.Value.RemoveService(service);
            }
        }

        public Service GetService(string[] route, out string matchedPath)
        {
            ChildrenContainer container = System.Threading.Volatile.Read(ref _children);
            if(route.Length == 0)
            {
                matchedPath = Path;
                return Services.GetService();
            }

            Node child;
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

            var children = System.Threading.Volatile.Read(ref _children);

            if(children.Count == 0) return;

            foreach (var kv in children)
            {
                if (kv.Value.Services.Count > 0)
                {
                    canCompress = false;
                    break;
                }
            }

            if (canCompress)
            {
                var newMerged = new ChildrenContainer(children.KeyLength + 1);
                foreach (var kv in children)
                {
                    foreach (var childkv in kv.Value.Children)
                    {
                        //This will prune out orphaned trees
                        if (childkv.Value.Services.Count > 0 || childkv.Value.Children.Count > 0)
                        {
                            var mergedKey = Enumerable.Concat(kv.Key, childkv.Key).ToArray();
                            var newNode = childkv.Value.CloneWithNewPrefix(mergedKey, Path);
                            newMerged[newNode.Prefix] = newNode;
                        }
                    }
                }
                System.Threading.Volatile.Write(ref _children, newMerged);
                Compress();
            }
            else
            {
                foreach (var kv in children)
                {
                    kv.Value.Compress();
                }
            }
        }

        public override string ToString()
        {
            return $"Path {string.Join("/",Prefix)} Services {Services.Count}";
        }
    }
}
