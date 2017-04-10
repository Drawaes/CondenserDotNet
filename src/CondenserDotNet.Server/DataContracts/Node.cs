using System.Collections.Generic;

namespace CondenserDotNet.Server.DataContracts
{
    public class Node
    {
        public string Path { get; set; }
        public string Services { get; set; }
        public string[] Prefix { get; set; }
        public Dictionary<string, Node> Nodes { get; set; }
    }
}